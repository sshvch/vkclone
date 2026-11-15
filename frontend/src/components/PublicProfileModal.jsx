import { useEffect, useState } from 'react';
import { getErrorMessage, usersApi } from '../api/client';
import { friendsLabel, getFullName } from '../utils/user';
import UserAvatar from './UserAvatar';

function PublicProfileModal({ userId, onClose, onUnauthorized }) {
  const [profile, setProfile] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;

    usersApi
      .get(`/users/${userId}`)
      .then(({ data }) => {
        if (!cancelled) setProfile(data);
      })
      .catch((err) => {
        if (cancelled) return;
        if (err.response?.status === 401) onUnauthorized?.();
        setError(getErrorMessage(err, 'Не удалось загрузить профиль'));
      });

    return () => {
      cancelled = true;
    };
  }, [userId, onUnauthorized]);

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(event) => event.stopPropagation()}>
        <button type="button" className="modal__close" onClick={onClose}>
          ×
        </button>

        {error && <div className="message error">{error}</div>}

        {!profile && !error && <p className="card__empty">Загружаем профиль...</p>}

        {profile && (
          <>
            <div className="modal__head">
              <UserAvatar user={profile} />
              <div>
                <div className="modal__name">{getFullName(profile)}</div>
                <div className="modal__meta">{profile.city || 'Город не указан'}</div>
              </div>
            </div>

            <dl className="info-list modal__rows">
              <div className="info-row">
                <dt>Статус</dt>
                <dd className={profile.status ? '' : 'info-row__empty'}>
                  {profile.status || 'Не указан'}
                </dd>
              </div>
              <div className="info-row">
                <dt>Друзья</dt>
                <dd>{friendsLabel(profile.friendsCount ?? 0)}</dd>
              </div>
              <div className="info-row">
                <dt>В друзьях</dt>
                <dd>{profile.isFriend ? 'Да' : 'Нет'}</dd>
              </div>
            </dl>
          </>
        )}
      </div>
    </div>
  );
}

export default PublicProfileModal;
