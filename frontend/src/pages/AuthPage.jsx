import { useState } from 'react';
import axios from 'axios';
import { API_URL, getErrorMessage } from '../api/client';

function AuthPage({ onAuthorized }) {
  const [isLoginView, setIsLoginView] = useState(true);

  const [loginData, setLoginData] = useState({ email: '', password: '' });
  const [regData, setRegData] = useState({ firstName: '', lastName: '', email: '', password: '' });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState({ text: '', type: '' });

  const handleLogin = async (e) => {
    e.preventDefault();
    setMessage({ text: '', type: '' });
    setIsSubmitting(true);
    try {
      const res = await axios.post(`${API_URL}/auth/login`, loginData);
      if (!res.data.token) {
        throw new Error('Сервер не вернул токен');
      }
      onAuthorized(res.data.token);
    } catch (error) {
      setMessage({ text: getErrorMessage(error, 'Ошибка входа'), type: 'error' });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setMessage({ text: '', type: '' });
    setIsSubmitting(true);
    try {
      await axios.post(`${API_URL}/auth/regist`, regData);
      setMessage({ text: 'Регистрация прошла успешно! Теперь войдите.', type: 'success' });
      setLoginData({ email: regData.email, password: '' });
      setRegData({ firstName: '', lastName: '', email: '', password: '' });
      setIsLoginView(true);
    } catch (error) {
      setMessage({ text: getErrorMessage(error, 'Ошибка регистрации'), type: 'error' });
    } finally {
      setIsSubmitting(false);
    }
  };

  const switchView = (toLogin) => {
    setIsLoginView(toLogin);
    setMessage({ text: '', type: '' });
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-logo">VK</div>
        <h2>{isLoginView ? 'Вход в аккаунт' : 'Создать аккаунт'}</h2>

        {message.text && (
          <div className={`message ${message.type}`}>{message.text}</div>
        )}

        {isLoginView ? (
          <form onSubmit={handleLogin} className="auth-form">
            <input
              type="email"
              placeholder="Email"
              autoComplete="email"
              required
              value={loginData.email}
              onChange={(e) => setLoginData({ ...loginData, email: e.target.value })}
            />
            <input
              type="password"
              placeholder="Пароль"
              autoComplete="current-password"
              required
              value={loginData.password}
              onChange={(e) => setLoginData({ ...loginData, password: e.target.value })}
            />
            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Входим...' : 'Войти'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleRegister} className="auth-form">
            <input
              placeholder="Имя"
              autoComplete="given-name"
              required
              value={regData.firstName}
              onChange={(e) => setRegData({ ...regData, firstName: e.target.value })}
            />
            <input
              placeholder="Фамилия"
              autoComplete="family-name"
              required
              value={regData.lastName}
              onChange={(e) => setRegData({ ...regData, lastName: e.target.value })}
            />
            <input
              type="email"
              placeholder="Email"
              autoComplete="email"
              required
              value={regData.email}
              onChange={(e) => setRegData({ ...regData, email: e.target.value })}
            />
            <input
              type="password"
              placeholder="Пароль"
              autoComplete="new-password"
              minLength={6}
              required
              value={regData.password}
              onChange={(e) => setRegData({ ...regData, password: e.target.value })}
            />
            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Создаём...' : 'Зарегистрироваться'}
            </button>
          </form>
        )}

        <div className="switch-view">
          {isLoginView ? (
            <p>
              Нет аккаунта?{' '}
              <span onClick={() => switchView(false)}>Зарегистрироваться</span>
            </p>
          ) : (
            <p>
              Уже есть аккаунт?{' '}
              <span onClick={() => switchView(true)}>Войти</span>
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

export default AuthPage;
