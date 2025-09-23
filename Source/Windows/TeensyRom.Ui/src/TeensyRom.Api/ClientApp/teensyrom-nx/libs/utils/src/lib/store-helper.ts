export function createAction(message: string): string {
  const randomInt = Math.floor(Math.random() * 10000);
  return `${message} [${randomInt}]`;
}
