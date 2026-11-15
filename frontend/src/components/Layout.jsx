import './Layout.css';
import './../pages/ProfilePage.css';
import { resolveAvatarUrl } from '../api/client';
import { getFullName, getInitials } from '../utils/user';

const TITLES = {
  profile: 'Моя страница',
  feed: 'Лента',
  friends: 'Друзья',
  notifications: 'Уведомления',
  chat: 'Сообщения',
};

const NAV_ITEMS = [
  { id: 'profile', label: 'Моя страница' },
  { id: 'feed', label: 'Лента' },
  { id: 'friends', label: 'Друзья', countKey: 'friends' },
  { id: 'notifications', label: 'Уведомления', countKey: 'unread' },
  { id: 'chat', label: 'Сообщения' },
];

function Layout({
  user,
  activeView,
  onNavigate,
  onLogout,
  friendsCount = 0,
  unreadCount = 0,
  children,
}) {
  const fullName = getFullName(user);

  const counts = { friends: friendsCount, unread: unreadCount };

  return (
    <div className="profile-page">
      <header className="topbar">
        <div className="topbar__inner">
          <div className="topbar__brand">
            <span className="topbar__logo">VK</span>
            <span className="topbar__title">{TITLES[activeView] || 'VK'}</span>
          </div>
          <div className="topbar__user">
            <span className="topbar__name">{fullName}</span>
            <span className="topbar__avatar">
              {user.avatarUrl ? (
                <img src={resolveAvatarUrl(user.avatarUrl)} alt={fullName} />
              ) : (
                getInitials(user)
              )}
            </span>
            <button type="button" className="topbar__logout" onClick={onLogout}>
              Выйти
            </button>
          </div>
        </div>
      </header>

      <main className="profile-main">
        <div className="layout-grid">
          <aside className="side-nav">
            {NAV_ITEMS.map((item) => {
              const count = item.countKey ? counts[item.countKey] : 0;

              return (
                <button
                  key={item.id}
                  type="button"
                  className={`side-nav__item${activeView === item.id ? ' side-nav__item_active' : ''}`}
                  onClick={() => onNavigate(item.id)}
                >
                  {item.label}
                  {count > 0 && <span className="side-nav__count">{count}</span>}
                </button>
              );
            })}
          </aside>

          <div className="layout-content">{children}</div>
        </div>
      </main>
    </div>
  );
}

export default Layout;
