export const KIND = {
  lecture: "CM",
  tutorial: "TD",
  practical: "TP",
  exam: "Examen",
  other: "Autre",
};

const ESCAPES = { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" };

export const esc = (value) => String(value ?? "").replace(/[&<>"']/g, (c) => ESCAPES[c]);

export class ApiError extends Error {
  constructor(status, message) {
    super(message);
    this.status = status;
  }
}

export async function api(path) {
  let response;
  try {
    response = await fetch(`/api/v1${path}`, { headers: { Accept: "application/json" } });
  } catch {
    throw new ApiError(0, "Le serveur ne répond pas. Vérifie ta connexion.");
  }
  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(response.status, body?.detail ?? `Erreur ${response.status}`);
  }
  return response.json();
}

export const storage = {
  get(key) {
    try {
      return JSON.parse(localStorage.getItem(key));
    } catch {
      return null;
    }
  },
  set(key, value) {
    try {
      localStorage.setItem(key, JSON.stringify(value));
    } catch {}
  },
};

export const parseDate = (iso) => {
  const [y, m, d] = iso.split("-").map(Number);
  return new Date(y, m - 1, d);
};

export const isoDate = (date) =>
  `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;

export const minutes = (hhmm) => {
  const [h, m] = hhmm.split(":").map(Number);
  return h * 60 + m;
};

export const nowMinutes = (date = new Date()) => date.getHours() * 60 + date.getMinutes();

export const addDays = (date, days) => {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
};

export const startOfWeek = (date) => {
  const monday = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  monday.setDate(monday.getDate() - ((monday.getDay() + 6) % 7));
  return monday;
};

const capitalize = (text) => text.charAt(0).toUpperCase() + text.slice(1);
const format = (options) => new Intl.DateTimeFormat("fr-FR", options);

export const weekdayShort = (date) => capitalize(format({ weekday: "short" }).format(date).replace(".", ""));
export const weekdayLong = (date) => capitalize(format({ weekday: "long" }).format(date));
export const dayMonth = (date) => format({ day: "numeric", month: "long" }).format(date);

export const daysBetween = (from, to) =>
  Math.round((parseDate(isoDate(to)) - parseDate(isoDate(from))) / 86400000);

export function duration(totalMinutes) {
  const h = Math.floor(totalMinutes / 60);
  const m = totalMinutes % 60;
  if (h === 0) return `${m} min`;
  return m === 0 ? `${h} h` : `${h} h ${String(m).padStart(2, "0")}`;
}

export function sinceLabel(isoTimestamp) {
  const elapsed = Math.max(0, Math.round((Date.now() - new Date(isoTimestamp)) / 60000));
  if (elapsed < 1) return "à l'instant";
  if (elapsed < 60) return `il y a ${elapsed} min`;
  return `il y a ${duration(elapsed)}`;
}

export const dateTimeLabel = (isoTimestamp) =>
  format({ day: "numeric", month: "long", hour: "2-digit", minute: "2-digit" }).format(new Date(isoTimestamp));

export const isLongRoom = (room) => room.length > 6;
