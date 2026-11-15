import { useCallback, useEffect, useState } from 'react';
import AuthPage from './pages/AuthPage';
import ProfilePage from './pages/ProfilePage';
import FeedPage from './pages/FeedPage';
import FriendsPage from './pages/FriendsPage';
import NotificationsPage from './pages/NotificationsPage';
import ChatPage from './pages/ChatPage';
import Layout from './components/Layout';
import { api, getErrorMessage, tokenStorage, usersApi } from './api/client';
import './App.css';

function App() {
  const [token, setToken] = useState(() => tokenStorage.get());
  const [user, setUser] = useState(null);
  const [view, setView] = useState('profile');
  const [unreadCount, setUnreadCount] = useState(0);
  const [isLoading, setIsLoading] = useState(() => Boolean(tokenStorage.get()));
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [error, setError] = useState('');

  const handleLogout = useCallback(() => {
    tokenStorage.clear();
    setToken(null);
    setUser(null);
    setView('profile');
    setUnreadCount(0);
  }, []);

  const fetchProfile = useCallback(async () => {
    try {
      const { data } = await usersApi.get('/users/profile');
      return data;
    } catch (err) {
      if (err.response?.status !== 404) throw err;

      const { data } = await api.get('/auth/me');
      return data;
    }
  }, []);

  const loadProfile = useCallback(async () => {
    setIsLoading(true);
    setError('');

    try {
      setUser(await fetchProfile());
    } catch (err) {
      if (err.response?.status === 401) {
        handleLogout();
      } else {
        setError(getErrorMessage(err, 'Не удалось загрузить профиль'));
      }
    } finally {
      setIsLoading(false);
    }
  }, [fetchProfile, handleLogout]);

  const refreshProfile = useCallback(async () => {
    setIsRefreshing(true);

    try {
      setUser(await fetchProfile());
    } catch (err) {
      if (err.response?.status === 401) handleLogout();
    } finally {
      setIsRefreshing(false);
    }
  }, [fetchProfile, handleLogout]);

  const refreshUnread = useCallback(async () => {
    try {
      const { data } = await usersApi.get('/users/me/friend-requests');
      setUnreadCount(data.filter((request) => !request.isRead).length);
    } catch (err) {
      if (err.response?.status === 401) handleLogout();
    }
  }, [handleLogout]);

  useEffect(() => {
    if (token) {
      loadProfile();
      refreshUnread();
    }
  }, [token, loadProfile, refreshUnread]);

  const handleSocialChanged = useCallback(() => {
    refreshUnread();
    refreshProfile();
  }, [refreshUnread, refreshProfile]);

  const handleAuthorized = (newToken) => {
    tokenStorage.save(newToken);
    setUser(null);
    setToken(newToken);
  };

  if (!token) {
    return <AuthPage onAuthorized={handleAuthorized} />;
  }

  if (error && !user) {
    return (
      <div className="centered-screen">
        <div className="message error">{error}</div>
        <div className="screen-actions">
          <button type="button" className="btn-primary" onClick={loadProfile}>
            Повторить
          </button>
          <button type="button" className="btn-link" onClick={handleLogout}>
            Выйти
          </button>
        </div>
      </div>
    );
  }

  if (isLoading || !user) {
    return (
      <div className="centered-screen">
        <div className="loader" />
        <p>Загружаем профиль...</p>
      </div>
    );
  }

  return (
    <Layout
      user={user}
      activeView={view}
      onNavigate={setView}
      onLogout={handleLogout}
      friendsCount={user.friendsCount ?? 0}
      unreadCount={unreadCount}
    >
      {view === 'profile' && (
        <ProfilePage
          user={user}
          onRefresh={refreshProfile}
          onUnauthorized={handleLogout}
          refreshing={isRefreshing}
        />
      )}

      {view === 'feed' && (
        <FeedPage currentUser={user} onUnauthorized={handleLogout} />
      )}

      {view === 'friends' && (
        <FriendsPage onFriendsChanged={refreshProfile} onUnauthorized={handleLogout} />
      )}

      {view === 'notifications' && (
        <NotificationsPage onChanged={handleSocialChanged} onUnauthorized={handleLogout} />
      )}

      {view === 'chat' && <ChatPage currentUser={user} onUnauthorized={handleLogout} />}
    </Layout>
  );
}

export default App;
