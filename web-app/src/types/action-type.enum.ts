/**
 * ActionType enum values matching C# backend
 * These values must match the backend enum exactly
 */
export enum ActionType {
  Add = 0,
  Update = 1,
  Delete = 2,
  NoAction = 3
}

export const ACTION_TYPE = {
  ADD: 0,
  UPDATE: 1,
  DELETE: 2,
  NO_ACTION: 3
} as const;