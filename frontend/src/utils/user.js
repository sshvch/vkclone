export function getInitials(user = {}) {
  const first = user.firstName?.[0] || '';
  const last = user.lastName?.[0] || '';
  return (first + last).toUpperCase() || '?';
}

export function getFullName(user = {}) {
  return `${user.firstName || ''} ${user.lastName || ''}`.trim() || 'Без имени';
}

export function pluralize(count, one, few, many) {
  const mod10 = count % 10;
  const mod100 = count % 100;

  if (mod10 === 1 && mod100 !== 11) return one;
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return few;
  return many;
}

export function friendsLabel(count) {
  return `${count} ${pluralize(count, 'друг', 'друга', 'друзей')}`;
}

export function formatDate(value) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';

  return date.toLocaleDateString('ru-RU', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  });
}

export function formatDateTime(value) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';

  return date.toLocaleString('ru-RU', {
    day: 'numeric',
    month: 'long',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function formatTime(value) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';

  return date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
}
