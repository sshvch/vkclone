import { usersApi } from './client';

export async function fetchAuthors(ids, currentUser) {
  const authors = {};

  for (const id of ids) {
    if (!id || authors[id]) continue;

    if (currentUser?.id === id) {
      authors[id] = currentUser;
      continue;
    }

    try {
      const { data } = await usersApi.get(`/users/${id}`);
      authors[id] = data;
    } catch {
      authors[id] = null;
    }
  }

  return authors;
}
