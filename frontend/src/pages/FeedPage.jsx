import { useCallback, useEffect, useState } from 'react';
import { contentApi, getErrorMessage } from '../api/client';
import { fetchAuthors } from '../api/authors';
import { formatDateTime, getFullName } from '../utils/user';
import PostComments from '../components/PostComments';
import PublicProfileModal from '../components/PublicProfileModal';
import UserAvatar from '../components/UserAvatar';
import './FeedPage.css';

const PAGE_SIZE = 10;
const EMPTY_FORM = { text: '', imageUrl: '' };

function FeedPage({ currentUser, onUnauthorized }) {
  const [posts, setPosts] = useState([]);
  const [authors, setAuthors] = useState({});
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState(EMPTY_FORM);
  const [publishing, setPublishing] = useState(false);
  const [likePending, setLikePending] = useState(null);
  const [deletingId, setDeletingId] = useState(null);
  const [openComments, setOpenComments] = useState({});
  const [selectedUserId, setSelectedUserId] = useState(null);

  const loadFeed = useCallback(
    async (targetPage) => {
      setLoading(true);
      setError('');

      try {
        const { data } = await contentApi.get('/posts/feed', {
          params: { page: targetPage, pageSize: PAGE_SIZE },
        });

        setPosts(data.items);
        setTotal(data.total);
        setPage(data.page);
        setAuthors(await fetchAuthors(data.items.map((post) => post.authorId), currentUser));
      } catch (err) {
        if (err.response?.status === 401) onUnauthorized?.();
        setError(getErrorMessage(err, 'Не удалось загрузить ленту'));
      } finally {
        setLoading(false);
      }
    },
    [currentUser, onUnauthorized],
  );

  useEffect(() => {
    loadFeed(1);
  }, [loadFeed]);

  const publish = async (event) => {
    event.preventDefault();

    if (!form.text.trim()) return;

    setPublishing(true);
    setError('');

    try {
      await contentApi.post('/posts', {
        text: form.text,
        imageUrl: form.imageUrl.trim() || null,
      });

      setForm(EMPTY_FORM);
      await loadFeed(1);
    } catch (err) {
      if (err.response?.status === 401) onUnauthorized?.();
      setError(getErrorMessage(err, 'Не удалось опубликовать пост'));
    } finally {
      setPublishing(false);
    }
  };

  const toggleComments = (postId) => {
    setOpenComments((prev) => ({ ...prev, [postId]: !prev[postId] }));
  };

  const toggleLike = async (post) => {
    const liked = Boolean(post.likes?.includes(currentUser.id));

    setLikePending(post.id);
    setError('');

    try {
      const { data } = liked
        ? await contentApi.delete(`/posts/${post.id}/like`)
        : await contentApi.post(`/posts/${post.id}/like`);

      setPosts((prev) => prev.map((item) => (item.id === post.id ? data : item)));
    } catch (err) {
      if (err.response?.status === 401) onUnauthorized?.();
      setError(getErrorMessage(err, liked ? 'Не удалось убрать лайк' : 'Не удалось поставить лайк'));
    } finally {
      setLikePending(null);
    }
  };

  const deletePost = async (post) => {
    if (!window.confirm('Удалить пост?')) return;

    setDeletingId(post.id);
    setError('');

    try {
      await contentApi.delete(`/posts/${post.id}`);
      setPosts((prev) => prev.filter((item) => item.id !== post.id));
      setTotal((prev) => Math.max(0, prev - 1));
    } catch (err) {
      if (err.response?.status === 401) onUnauthorized?.();
      setError(getErrorMessage(err, 'Не удалось удалить пост'));
    } finally {
      setDeletingId(null);
    }
  };

  const pagesCount = Math.max(1, Math.ceil(total / PAGE_SIZE));

  return (
    <div>
      <div className="page-head">
        <h1>Лента</h1>
        <button
          type="button"
          className="btn-friend btn-friend_secondary"
          onClick={() => loadFeed(page)}
          disabled={loading}
        >
          Обновить
        </button>
      </div>

      {error && <div className="message error">{error}</div>}

      <form className="post-form" onSubmit={publish}>
        <textarea
          value={form.text}
          onChange={(event) => setForm({ ...form, text: event.target.value })}
          placeholder="Что у вас нового?"
          rows={3}
          maxLength={5000}
        />
        <input
          type="url"
          value={form.imageUrl}
          onChange={(event) => setForm({ ...form, imageUrl: event.target.value })}
          placeholder="Ссылка на картинку (необязательно)"
        />
        <button type="submit" disabled={publishing || !form.text.trim()}>
          {publishing ? 'Публикуем...' : 'Опубликовать'}
        </button>
      </form>

      {loading && <div className="empty-state">Загружаем ленту...</div>}

      {!loading && posts.length === 0 && (
        <div className="empty-state">Постов пока нет. Опубликуйте первый!</div>
      )}

      {!loading &&
        posts.map((post) => {
          const author = authors[post.authorId];
          const commentsOpen = Boolean(openComments[post.id]);
          const liked = Boolean(post.likes?.includes(currentUser.id));
          const isOwn = post.authorId === currentUser.id;

          return (
            <article key={post.id} className="post-card">
              <header className="post-card__head">
                <button
                  type="button"
                  className="post-card__author"
                  onClick={() => setSelectedUserId(post.authorId)}
                >
                  <UserAvatar user={author || {}} />
                  <span className="post-card__author-text">
                    <span className="post-card__name">
                      {author ? getFullName(author) : 'Пользователь'}
                    </span>
                    <span className="post-card__date">{formatDateTime(post.createdAt)}</span>
                  </span>
                </button>
              </header>

              <p className="post-card__text">{post.text}</p>

              {post.imageUrl && (
                <img className="post-card__image" src={post.imageUrl} alt="" />
              )}

              <footer className="post-card__actions">
                <button
                  type="button"
                  className={`post-card__like${liked ? ' post-card__like_active' : ''}`}
                  onClick={() => toggleLike(post)}
                  disabled={likePending === post.id}
                >
                  {liked ? '♥' : '♡'} {post.likesCount ?? 0}
                </button>

                <div className="post-card__buttons">
                  <button
                    type="button"
                    className="post-card__comments-toggle"
                    onClick={() => toggleComments(post.id)}
                  >
                    {commentsOpen ? 'Скрыть комментарии' : 'Комментарии'}
                  </button>

                  {isOwn && (
                    <button
                      type="button"
                      className="post-card__delete"
                      onClick={() => deletePost(post)}
                      disabled={deletingId === post.id}
                    >
                      {deletingId === post.id ? 'Удаляем...' : 'Удалить'}
                    </button>
                  )}
                </div>
              </footer>

              {commentsOpen && (
                <PostComments
                  postId={post.id}
                  currentUser={currentUser}
                  onUnauthorized={onUnauthorized}
                />
              )}
            </article>
          );
        })}

      {!loading && total > PAGE_SIZE && (
        <div className="pagination">
          <button
            type="button"
            className="btn-friend btn-friend_secondary"
            disabled={page <= 1}
            onClick={() => loadFeed(page - 1)}
          >
            Назад
          </button>
          <span className="card__empty">
            Страница {page} из {pagesCount}
          </span>
          <button
            type="button"
            className="btn-friend btn-friend_secondary"
            disabled={page >= pagesCount}
            onClick={() => loadFeed(page + 1)}
          >
            Вперёд
          </button>
        </div>
      )}

      {selectedUserId && (
        <PublicProfileModal
          userId={selectedUserId}
          onClose={() => setSelectedUserId(null)}
          onUnauthorized={onUnauthorized}
        />
      )}
    </div>
  );
}

export default FeedPage;
