import { useState } from 'react';
import { getErrorMessage, resolveAvatarUrl, usersApi } from '../api/client';
import { formatDate, getFullName, getInitials, pluralize } from '../utils/user';
import './ProfilePage.css';

function InfoRow({ label, value }) {
  return (
    <div className="info-row">
      <dt>{label}</dt>
      <dd className={value ? '' : 'info-row__empty'}>{value || 'Не указано'}</dd>
    </div>
  );
}

function ProfilePage({ user, onRefresh, onUnauthorized, refreshing = false }) {
  const fullName = getFullName(user);
  const registeredAt = formatDate(user.createdAt);
  const friendsCount = user.friendsCount ?? 0;

  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState(null);
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');

  const startEdit = () => {
    setForm({
      firstName: user.firstName || '',
      lastName: user.lastName || '',
      city: user.city || '',
      phone: user.phone || '',
      status: user.status || '',
    });
    setError('');
    setEditing(true);
  };

  const cancelEdit = () => {
    setEditing(false);
    setError('');
  };

  const save = async (event) => {
    event.preventDefault();

    setSaving(true);
    setError('');

    try {
      await usersApi.put('/users/profile', form);
      setEditing(false);
      await onRefresh?.();
    } catch (err) {
      if (err.response?.status === 401) {
        onUnauthorized?.();
        return;
      }

      setError(getErrorMessage(err, 'Не удалось сохранить профиль'));
    } finally {
      setSaving(false);
    }
  };

  const uploadAvatar = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const data = new FormData();
    data.append('file', file);

    setUploading(true);
    setError('');

    try {
      await usersApi.post('/users/profile/avatar', data);
      await onRefresh?.();
    } catch (err) {
      if (err.response?.status === 401) {
        onUnauthorized?.();
        return;
      }

      setError(getErrorMessage(err, 'Не удалось загрузить аватар'));
    } finally {
      setUploading(false);
      event.target.value = '';
    }
  };

  const removeAvatar = async () => {
    setUploading(true);
    setError('');

    try {
      await usersApi.delete('/users/profile/avatar');
      await onRefresh?.();
    } catch (err) {
      if (err.response?.status === 401) {
        onUnauthorized?.();
        return;
      }

      setError(getErrorMessage(err, 'Не удалось удалить аватар'));
    } finally {
      setUploading(false);
    }
  };

  return (
    <>
      <section className="hero">
        <div className="hero__cover" />
        <div className="hero__body">
          <div className="avatar">
            {user.avatarUrl ? (
              <img src={resolveAvatarUrl(user.avatarUrl)} alt={fullName} />
            ) : (
              <span>{getInitials(user)}</span>
            )}
          </div>

          <div className="hero__info">
            <h1>{fullName}</h1>
            <p className={user.status ? 'hero__status' : 'hero__status hero__status_empty'}>
              {user.status || 'Статус пока не установлен'}
            </p>
            <div className="hero__meta">
              <span className="online-dot" />
              <span>В сети</span>
              {user.city && (
                <>
                  <span className="hero__meta-sep">·</span>
                  <span>{user.city}</span>
                </>
              )}
            </div>
          </div>

          <div className="hero__actions">
            {!editing && (
              <button type="button" className="btn-secondary" onClick={startEdit}>
                Редактировать
              </button>
            )}
            <button
              type="button"
              className="btn-secondary"
              onClick={onRefresh}
              disabled={refreshing}
            >
              {refreshing ? 'Обновляем...' : 'Обновить'}
            </button>
          </div>
        </div>
      </section>

      <div className="profile-body">
        <section className="profile-content">
          {editing ? (
            <form className="card profile-form" onSubmit={save}>
              <h2 className="card__title">Редактирование профиля</h2>

              <div className="profile-form__grid">
                <label className="field">
                  <span>Имя</span>
                  <input
                    value={form.firstName}
                    onChange={(event) => setForm({ ...form, firstName: event.target.value })}
                    maxLength={50}
                    required
                  />
                </label>

                <label className="field">
                  <span>Фамилия</span>
                  <input
                    value={form.lastName}
                    onChange={(event) => setForm({ ...form, lastName: event.target.value })}
                    maxLength={50}
                    required
                  />
                </label>

                <label className="field">
                  <span>Город</span>
                  <input
                    value={form.city}
                    onChange={(event) => setForm({ ...form, city: event.target.value })}
                    maxLength={100}
                  />
                </label>

                <label className="field">
                  <span>Телефон</span>
                  <input
                    type="tel"
                    value={form.phone}
                    onChange={(event) => setForm({ ...form, phone: event.target.value })}
                    maxLength={30}
                  />
                </label>

                <div className="field field_wide">
                  <span>Аватар</span>
                  <div className="avatar-upload">
                    <input
                      type="file"
                      accept="image/jpeg,image/png,image/webp,image/gif"
                      onChange={uploadAvatar}
                      disabled={uploading}
                    />
                    {user.avatarUrl && (
                      <button
                        type="button"
                        className="btn-secondary"
                        onClick={removeAvatar}
                        disabled={uploading}
                      >
                        {uploading ? 'Подождите...' : 'Удалить фото'}
                      </button>
                    )}
                  </div>
                </div>

                <label className="field field_wide">
                  <span>Статус</span>
                  <input
                    value={form.status}
                    onChange={(event) => setForm({ ...form, status: event.target.value })}
                    maxLength={200}
                  />
                </label>
              </div>

              {error && <div className="message error">{error}</div>}

              <div className="profile-form__actions">
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={cancelEdit}
                  disabled={saving}
                >
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={saving}>
                  {saving ? 'Сохраняем...' : 'Сохранить'}
                </button>
              </div>
            </form>
          ) : (
            <>
              <div className="card">
                <h2 className="card__title">Основная информация</h2>
                <dl className="info-list">
                  <InfoRow label="Город" value={user.city} />
                  <InfoRow label="Телефон" value={user.phone} />
                  <InfoRow label="Email" value={user.email} />
                  <InfoRow label="Дата регистрации" value={registeredAt} />
                </dl>
              </div>

              <div className="card">
                <h2 className="card__title">О себе</h2>
                <p className={user.status ? '' : 'card__empty'}>
                  {user.status || 'Пока ничего не рассказано о себе.'}
                </p>
              </div>
            </>
          )}
        </section>

        <aside className="profile-side">
          <div className="card">
            <h2 className="card__title">Друзья</h2>
            <div className="stat">
              <span className="stat__value">{friendsCount}</span>
              <span className="stat__label">
                {pluralize(friendsCount, 'друг', 'друга', 'друзей')}
              </span>
            </div>
          </div>

          <div className="card">
            <h2 className="card__title">Аккаунт</h2>
            <p className="account-id">
              ID: <code>{user.id}</code>
            </p>
          </div>
        </aside>
      </div>
    </>
  );
}

export default ProfilePage;
