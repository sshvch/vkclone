import { getFullName, getInitials } from '../utils/user';
import { resolveAvatarUrl } from '../api/client';

function UserAvatar({ user }) {
  return (
    <div className="user-avatar">
      {user.avatarUrl ? (
        <img src={resolveAvatarUrl(user.avatarUrl)} alt={getFullName(user)} />
      ) : (
        getInitials(user)
      )}
    </div>
  );
}

export default UserAvatar;
