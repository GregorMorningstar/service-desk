export const ROLES = ['Administrator', 'Moderator', 'User'] as const;
export type Role = (typeof ROLES)[number];

export const ROLE_LABELS: Record<Role, string> = {
  Administrator: 'Administrator',
  Moderator: 'Moderator',
  User: 'Użytkownik',
};
