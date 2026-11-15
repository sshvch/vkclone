import { useCallback, useEffect, useState } from 'react';
import { getErrorMessage, usersApi } from '../api/client';
import { friendsLabel, getFullName } from '../utils/user';
import PublicProfileModal from '../components/PublicProfileModal';
import UserAvatar from '../components/UserAvatar';
import './SocialPages.css';

const PAGE_SIZE = 12;

function FriendsPage({ onFriendsChanged, onUnauthorized }) {
  const [tab, setTab] = useState('all');
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [users, setUsers] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionId, setActionId] = useState(null);
  const [selectedId, setSelectedId] = useState(null);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(search.trim()), 350);
    return () => clearTimeout(timer);
  }, [search]);

  const loadAll = useCallback(
    async (targetPage) => {
      setLoading(true);
      setError('');

      try {
        const { data } = await usersApi.get('/users', {
          params: {
            page: targetPage,
            pageSize: PAGE_SIZE,
            search: debouncedSearch || undefined,
          },
        });

        setUsers(data.items);
        setTotal(data.total);
        setPage(data.page);
      } catch (err) {
        if (err.response?.status === 401) onUnauthorized?.();
        setError(getErrorMessage(err, 'Не удалось загрузить пользователей'));
      } finally {
        setLoading(false);
      }
    },
    [debouncedSearch, onUnauthorized],
  );

  const loadFriends = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const { data } = await usersApi.get('/users/me/friends');
      setUsers(data);
      setTotal(data.length);
    } catch (err) {
      if (err.response?.status === 401) onUnauthorized?.();
      setError(getErrorMessage(err, 'Не удалось загрузить друзей'));
    } finally {
      setLoading(false);
    }
  }, [onUnauthorized]);

  useEffect(() => {
    if (tab === 'all') loadAll(1);
  }, [tab, loadAll]);

  useEffect(() => {
    if (tab === 'friends') loadFriends();
  }, [tab, loadFriends]);

  const sendRequest = async (userId) => {
    setActionId(userId);
    setError('');

    try {
      await usersApi.post(`/users/me/friend-requests/${userId}`);
      setUsers((prev) =>
        prev.map((user) =>
          user.id === userId ? { ...user, hasOutgoingRequest: true } : user,
        ),
      );
    } catch (err) {
      setError(getErrorMessage(err, 'Не удалось отправить заявку'));
    } finally {
      setActionId(null);
    }
  };

  const removeFriend = async (userId) => {
    setActionId(userId);
    setError('');

    try {
      await usersApi.delete(`/users/me/friends/${userId}`);

      if (tab === 'friends') {
        setUsers((prev) => prev.filter((user) => user.id !== userId));
        setTotal((prev) => Math.max(0, prev - 1));
      } else {
        setUsers((prev) =>
          prev.map((user) =>
            user.id === userId
              ? { ...user, isFriend: false, friendsCount: Math.max(0, user.friendsCount - 1) }
              : user,
          ),
        );
      }

      onFriendsChanged?.();
    } catch (err) {
      setError(getErrorMessage(err, 'Не удалось удалить из друзей'));
    } finally {
      setActionId(null);
    }
  };

  const pagesCount = Math.max(1, Math.ceil(total / PAGE_SIZE));

  return (
    <div>
      <div className="page-head">
        <div className="tabs">
          <button
            type="button"
            className={`tab${tab === 'all' ? ' tab_active' : ''}`}
            onClick={() => setTab('all')}
          >
            Все пользователи
          </button>
          <button
            type="button"
            className={`tab${tab === 'friends' ? ' tab_active' : ''}`}
            onClick={() => setTab('friends')}
          >
            Мои друзья
          </button>
        </div>

        {tab === 'all' && (
          <input
            type="search"
            className="search-input"
            placeholder="Поиск по имени или фамилии"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        )}
      </div>

      {error && <div className="message error">{error}</div>}

      {loading && <div className="empty-state">Загружаем...</div>}

      {!loading && users.length === 0 && (
        <div className="empty-state">
          {tab === 'friends'
            ? 'У вас пока нет друзей. Найдите их на вкладке «Все пользователи».'
            : 'Пользователи не найдены.'}
        </div>
      )}

      {!loading && users.length > 0 && (
        <div className="user-grid">
          {users.map((user) => (
            <article
              key={user.id}
              className="user-card"
              onClick={() => setSelectedId(user.id)}
            >
              <div className="user-card__head">
                <UserAvatar user={user} />
                <div>
                  <div className="user-card__name">{getFullName(user)}</div>
                  <div className="user-card__meta">
                    {user.city || 'Город не указан'} · {friendsLabel(user.friendsCount ?? 0)}
                  </div>
                </div>
              </div>

              <div className="user-card__actions" onClick={(event) => event.stopPropagation()}>
                {user.isFriend ? (
                  <button
                    type="button"
                    className="btn-friend btn-friend_danger"
                    disabled={actionId === user.id}
                    onClick={() => removeFriend(user.id)}
                  >
                    {actionId === user.id ? 'Удаляем...' : 'Удалить из друзей'}
                  </button>
                ) : user.hasOutgoingRequest ? (
                  <button type="button" className="btn-friend btn-friend_secondary" disabled>
                    Заявка отправлена
                  </button>
                ) : (
                  <button
                    type="button"
                    className="btn-friend"
                    disabled={actionId === user.id}
                    onClick={() => sendRequest(user.id)}
                  >
                    {actionId === user.id ? 'Отправляем...' : 'Добавить в друзья'}
                  </button>
                )}
              </div>
            </article>
          ))}
        </div>
      )}

      {!loading && tab === 'all' && total > PAGE_SIZE && (
        <div className="pagination">
          <button
            type="button"
            className="btn-friend btn-friend_secondary"
            disabled={page <= 1}
            onClick={() => loadAll(page - 1)}
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
            onClick={() => loadAll(page + 1)}
          >
            Вперёд
          </button>
        </div>
      )}

      {selectedId && (
        <PublicProfileModal
          userId={selectedId}
          onClose={() => setSelectedId(null)}
          onUnauthorized={onUnauthorized}
        />
      )}
    </div>
  );
}

export default FriendsPage;
