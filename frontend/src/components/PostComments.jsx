import { useEffect, useState } from 'react';
import { contentApi, getErrorMessage } from '../api/client';
import { fetchAuthors } from '../api/authors';
import { formatDateTime, getFullName } from '../utils/user';
import UserAvatar from './UserAvatar';

function PostComments({ postId, currentUser, onUnauthorized }) {
  const [comments, setComments] = useState([]);
  const [authors, setAuthors] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [text, setText] = useState('');
  const [sending, setSending] = useState(false);
  const [deletingId, setDeletingId] = useState(null);

  useEffect(() => {
    let cancelled = false;

    setLoading(true);
    setError('');

    contentApi
      .get(`/comments/${postId}`)
      .then(async ({ data }) => {
        if (cancelled) return;
        setComments(data);
        setAuthors(await fetchAuthors(data.map((comment) => comment.authorId), currentUser));
      })
      .catch((err) => {
        if (cancelled) return;
        if (err.response?.status === 401) onUnauthorized?.();
        setError(getErrorMessage(err, 'Не удалось загрузить комментарии'));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [postId, currentUser, onUnauthorized]);

  const send = async (event) => {
    event.preventDefault();

    if (!text.trim()) return;

    setSending(true);
    setError('');

    try {
      const { data } = await contentApi.post('/comments', { postId, text });
      setComments((prev) => [...prev, data]);
      setAuthors((prev) => ({ ...prev, [data.authorId]: currentUser }));
      setText('');
    } catch (err) {
      if (err.response?.status === 401) onUnauthorized?.();
      setError(getErrorMessage(err, 'Не удалось отправить комментарий'));
    } finally {
      setSending(false);
    }
  };

  const deleteComment = async (comment) => {
    if (!window.confirm('Удалить комментарий?')) return;

    setDeletingId(comment.id);
    setError('');

    try {
      await contentApi.delete(`/comments/${comment.id}`);
      setComments((prev) => prev.filter((item) => item.id !== comment.id));
    } catch (err) {
      if (err.response?.status === 401) onUnauthorized?.();
      setError(getErrorMessage(err, 'Не удалось удалить комментарий'));
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="comments">
      {error && <div className="message error">{error}</div>}

      {loading && <div className="comments__empty">Загружаем комментарии...</div>}

      {!loading && comments.length === 0 && (
        <div className="comments__empty">Комментариев пока нет.</div>
      )}

      {comments.map((comment) => {
        const author = authors[comment.authorId];

        return (
          <div key={comment.id} className="comment">
            <UserAvatar user={author || {}} />
            <div className="comment__body">
              <div className="comment__head">
                <span className="comment__name">
                  {author ? getFullName(author) : 'Пользователь'}
                </span>
                <span className="comment__date">{formatDateTime(comment.createdAt)}</span>
                {comment.authorId === currentUser.id && (
                  <button
                    type="button"
                    className="comment__delete"
                    onClick={() => deleteComment(comment)}
                    disabled={deletingId === comment.id}
                  >
                    {deletingId === comment.id ? 'Удаляем...' : 'Удалить'}
                  </button>
                )}
              </div>
              <p className="comment__text">{comment.text}</p>
            </div>
          </div>
        );
      })}

      <form className="comment-form" onSubmit={send}>
        <input
          value={text}
          onChange={(event) => setText(event.target.value)}
          placeholder="Написать комментарий"
          maxLength={2000}
        />
        <button type="submit" disabled={sending || !text.trim()}>
          {sending ? 'Отправляем...' : 'Отправить'}
        </button>
      </form>
    </div>
  );
}

export default PostComments;
