import { useEffect, useRef, useState } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { CHAT_HUB_URL, chatApi, getErrorMessage, tokenStorage, usersApi } from '../api/client';
import { formatTime, getFullName } from '../utils/user';
import UserAvatar from '../components/UserAvatar';
import './SocialPages.css';

const STATUS_LABELS = {
  connecting: 'подключаемся...',
  connected: 'на связи',
  disconnected: 'нет соединения',
};

function ChatPage({ currentUser, onUnauthorized }) {
  const [friends, setFriends] = useState([]);
  const [peer, setPeer] = useState(null);
  const [messages, setMessages] = useState([]);
  const [text, setText] = useState('');
  const [loadingFriends, setLoadingFriends] = useState(true);
  const [loadingHistory, setLoadingHistory] = useState(false);
  const [error, setError] = useState('');
  const [connectionState, setConnectionState] = useState('connecting');

  const connectionRef = useRef(null);
  const peerRef = useRef(null);
  const messagesRef = useRef(null);

  useEffect(() => {
    peerRef.current = peer;
  }, [peer]);

  useEffect(() => {
    let cancelled = false;

    usersApi
      .get('/users/me/friends')
      .then(({ data }) => {
        if (cancelled) return;
        setFriends(data);
        if (data.length > 0) setPeer(data[0]);
      })
      .catch((err) => {
        if (cancelled) return;
        if (err.response?.status === 401) onUnauthorized?.();
        setError(getErrorMessage(err, 'Не удалось загрузить друзей'));
      })
      .finally(() => {
        if (!cancelled) setLoadingFriends(false);
      });

    return () => {
      cancelled = true;
    };
  }, [onUnauthorized]);

  useEffect(() => {
    if (!peer) {
      setMessages([]);
      return undefined;
    }

    let cancelled = false;
    setLoadingHistory(true);
    setError('');

    chatApi
      .get(`/messages/${peer.id}`)
      .then(({ data }) => {
        if (!cancelled) setMessages(data);
      })
      .catch((err) => {
        if (cancelled) return;
        if (err.response?.status === 401) onUnauthorized?.();
        setError(getErrorMessage(err, 'Не удалось загрузить переписку'));
      })
      .finally(() => {
        if (!cancelled) setLoadingHistory(false);
      });

    return () => {
      cancelled = true;
    };
  }, [peer, onUnauthorized]);

  useEffect(() => {
    let cancelled = false;

    const connection = new HubConnectionBuilder()
      .withUrl(CHAT_HUB_URL, { accessTokenFactory: () => tokenStorage.get() ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('ReceiveMessage', (message) => {
      const currentPeer = peerRef.current;
      const related =
        currentPeer &&
        (message.fromUserId === currentPeer.id || message.toUserId === currentPeer.id);

      if (!related) return;

      setMessages((prev) =>
        prev.some((item) => item.id === message.id) ? prev : [...prev, message],
      );
    });

    connection.onreconnecting(() => setConnectionState('connecting'));
    connection.onreconnected(() => setConnectionState('connected'));
    connection.onclose(() => setConnectionState('disconnected'));

    connectionRef.current = connection;
    setConnectionState('connecting');

    connection
      .start()
      .then(() => {
        if (cancelled) {
          connection.stop();
          return;
        }
        setConnectionState('connected');
      })
      .catch((err) => {
        if (cancelled) return;
        setConnectionState('disconnected');
        setError(`Не удалось подключиться к чату: ${err.message}`);
      });

    return () => {
      cancelled = true;
      connectionRef.current = null;
      connection.stop();
    };
  }, []);

  useEffect(() => {
    const element = messagesRef.current;
    if (element) element.scrollTop = element.scrollHeight;
  }, [messages]);

  const sendMessage = async (event) => {
    event.preventDefault();

    const value = text.trim();
    const connection = connectionRef.current;

    if (!value || !peer || !connection || connectionState !== 'connected') return;

    setText('');
    setError('');

    try {
      await connection.invoke('SendMessage', peer.id, value);
    } catch (err) {
      setText(value);
      setError(getErrorMessage(err, 'Не удалось отправить сообщение'));
    }
  };

  return (
    <div>
      <div className="page-head">
        <h1>Сообщения</h1>
        <span className="chat-status">{STATUS_LABELS[connectionState]}</span>
      </div>

      {error && <div className="message error">{error}</div>}

      <div className="chat-layout">
        <aside className="chat-friends">
          <div className="chat-friends__title">Друзья</div>

          {loadingFriends && <p className="card__empty">Загружаем...</p>}

          {!loadingFriends && friends.length === 0 && (
            <p className="card__empty">Сначала добавьте кого-нибудь в друзья.</p>
          )}

          {friends.map((friend) => (
            <button
              key={friend.id}
              type="button"
              className={`chat-friend${peer?.id === friend.id ? ' chat-friend_active' : ''}`}
              onClick={() => setPeer(friend)}
            >
              <UserAvatar user={friend} />
              <span className="chat-friend__name">{getFullName(friend)}</span>
            </button>
          ))}
        </aside>

        <section className="chat-thread">
          {peer ? (
            <>
              <div className="chat-thread__head">
                <span>{getFullName(peer)}</span>
                <span className="chat-status">{STATUS_LABELS[connectionState]}</span>
              </div>

              <div className="chat-messages" ref={messagesRef}>
                {loadingHistory && <p className="chat-empty">Загружаем сообщения...</p>}

                {!loadingHistory && messages.length === 0 && (
                  <p className="chat-empty">Сообщений пока нет. Напишите первым.</p>
                )}

                {messages.map((message) => {
                  const own = message.fromUserId === currentUser.id;

                  return (
                    <div
                      key={message.id}
                      className={`chat-bubble${own ? ' chat-bubble_own' : ''}`}
                    >
                      <p className="chat-bubble__text">{message.text}</p>
                      <span className="chat-bubble__time">{formatTime(message.createdAt)}</span>
                    </div>
                  );
                })}
              </div>

              <form className="chat-form" onSubmit={sendMessage}>
                <input
                  type="text"
                  placeholder="Напишите сообщение..."
                  value={text}
                  onChange={(event) => setText(event.target.value)}
                  disabled={connectionState !== 'connected'}
                />
                <button
                  type="submit"
                  disabled={!text.trim() || connectionState !== 'connected'}
                >
                  Отправить
                </button>
              </form>
            </>
          ) : (
            <p className="chat-empty">Выберите друга, чтобы начать переписку.</p>
          )}
        </section>
      </div>
    </div>
  );
}

export default ChatPage;
