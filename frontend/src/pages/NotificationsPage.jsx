import { useCallback, useEffect, useState } from 'react';
import { getErrorMessage, usersApi } from '../api/client';
import { formatDateTime, getFullName } from '../utils/user';
import UserAvatar from '../components/UserAvatar';
import './SocialPages.css';

function NotificationsPage({ onChanged, onUnauthorized }) {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionId, setActionId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const { data } = await usersApi.get('/users/me/friend-requests');
      setItems(data);
    } catch (err) {
      if (err.response?.status === 401) onUnauthorized?.();
      setError(getErrorMessage(err, 'Не удалось загрузить заявки'));
    } finally {
      setLoading(false);
    }
  }, [onUnauthorized]);

  useEffect(() => {
    load();
  }, [load]);

  const handleAction = async (id, action) => {
    setActionId(id);
    setError('');

    try {
      await usersApi.post(`/users/me/friend-requests/${id}/${action}`);

      if (action === 'read') {
        setItems((prev) => prev.map((item) => (item.id === id ? { ...item, isRead: true } : item)));
      } else {
        setItems((prev) => prev.filter((item) => item.id !== id));
        onChanged?.();
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Не удалось обработать заявку'));
    } finally {
      setActionId(null);
    }
  };

  return (
    <div>
      <div className="page-head">
        <h1>Заявки в друзья</h1>
        <button
          type="button"
          className="btn-friend btn-friend_secondary"
          onClick={load}
          disabled={loading}
        >
          Обновить
        </button>
      </div>

      {error && <div className="message error">{error}</div>}

      {loading && <div className="empty-state">Загружаем...</div>}

      {!loading && items.length === 0 && (
        <div className="empty-state">Новых заявок в друзья нет.</div>
      )}

      {!loading && items.length > 0 && (
        <div className="request-list">
          {items.map((item) => {
            const sender = item.fromUser;

            return (
              <article key={item.id} className="request-item">
                <UserAvatar user={sender || {}} />

                <div className="request-item__info">
                  <div className="request-item__name">
                    {sender ? getFullName(sender) : 'Пользователь'}
                  </div>
                  <div className="request-item__date">
                    Заявка от {formatDateTime(item.createdAt)}
                    {item.isRead && <span className="badge-read"> · просмотрено</span>}
                  </div>
                </div>

                <div className="request-item__actions">
                  <button
                    type="button"
                    className="btn-friend"
                    disabled={actionId === item.id}
                    onClick={() => handleAction(item.id, 'accept')}
                  >
                    Принять
                  </button>
                  <button
                    type="button"
                    className="btn-friend btn-friend_danger"
                    disabled={actionId === item.id}
                    onClick={() => handleAction(item.id, 'decline')}
                  >
                    Отклонить
                  </button>
                  <button
                    type="button"
                    className="btn-friend btn-friend_secondary"
                    disabled={item.isRead || actionId === item.id}
                    onClick={() => handleAction(item.id, 'read')}
                  >
                    Просмотрено
                  </button>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </div>
  );
}

export default NotificationsPage;
