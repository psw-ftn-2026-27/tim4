// How the app writes dates, amounts and lengths, so every module writes them the same way
// (frontend/AGENTS.md, UI rules, "Dates and numbers"). Plain functions: a page calls them from a
// computed() or from the template through a protected member. The server sends a date as an ISO
// string and a decimal as a number; nothing else is ever shown raw.

const pad = (value: number) => String(value).padStart(2, '0');

// "21.11.2026."
export function formatDate(iso: string): string {
  const date = new Date(iso);
  return `${pad(date.getDate())}.${pad(date.getMonth() + 1)}.${date.getFullYear()}.`;
}

// "21.11.2026. 14:05"
export function formatDateTime(iso: string): string {
  const date = new Date(iso);
  return `${formatDate(iso)} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

// "37,5": at most one decimal, with a decimal comma, and none when it is whole ("40").
export function formatNumber(value: number): string {
  return String(Math.round(value * 10) / 10).replace('.', ',');
}

// "1,4 km" from kilometres; under one kilometre in metres, "850 m".
export function formatKm(km: number): string {
  return km < 1 ? `${Math.round(km * 1000)} m` : `${km.toFixed(1).replace('.', ',')} km`;
}

// "45 min", "1 h 30 min", "2 h".
export function formatMinutes(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  if (hours === 0) {
    return `${rest} min`;
  }
  return rest === 0 ? `${hours} h` : `${hours} h ${rest} min`;
}
