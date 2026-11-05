import {
  KIND, esc, minutes, nowMinutes, parseDate, isoDate, addDays, startOfWeek,
  weekdayShort, weekdayLong, dayMonth, duration, isLongRoom,
} from "./util.js";

const WEEK_MS = 7 * 86400000;

const CHEVRON_LEFT = `<svg viewBox="0 0 20 20" aria-hidden="true"><path d="M12.5 4l-6 6 6 6" fill="none" stroke="currentColor" stroke-width="2"/></svg>`;
const CHEVRON_RIGHT = `<svg viewBox="0 0 20 20" aria-hidden="true"><path d="M7.5 4l6 6-6 6" fill="none" stroke="currentColor" stroke-width="2"/></svg>`;

export function mountWeek(host, { courses, mode }) {
  const byDate = new Map();
  for (const course of courses) {
    if (!byDate.has(course.date)) byDate.set(course.date, []);
    byDate.get(course.date).push(course);
  }

  const today = new Date();
  const todayIso = isoDate(today);
  const thisWeek = startOfWeek(today);
  const weeks = courses.map((c) => startOfWeek(parseDate(c.date)).getTime());
  const minWeek = Math.min(thisWeek.getTime(), ...weeks);
  const maxWeek = Math.max(thisWeek.getTime(), ...weeks);

  let week = thisWeek;
  const weekend = today.getDay() === 6 || today.getDay() === 0;
  const nextWeek = addDays(thisWeek, 7);
  if (weekend && !courses.some((c) => c.date >= todayIso && parseDate(c.date) < nextWeek) && nextWeek.getTime() <= maxWeek) {
    week = nextWeek;
  }
  let selected = null;

  const render = () => {
    const days = daysOf(week, byDate);
    if (!days.some((d) => isoDate(d) === selected)) selected = defaultDay(days, todayIso, byDate);

    host.innerHTML =
      navHtml(week, thisWeek, minWeek, maxWeek) +
      agendaHtml(days, selected, byDate, todayIso, mode) +
      gridHtml(days, byDate, todayIso, mode);
  };

  host.addEventListener("click", (event) => {
    const control = event.target.closest("[data-action]");
    if (!control || control.disabled) return;

    const action = control.dataset.action;
    if (action === "prev") week = addDays(week, -7);
    if (action === "next") week = addDays(week, 7);
    if (action === "today") week = thisWeek;
    selected = action === "day" ? control.dataset.date : null;
    render();

    const again = action === "day"
      ? host.querySelector(`[data-action="day"][data-date="${selected}"]`)
      : host.querySelector(`[data-action="${action}"]:not(:disabled)`);
    (again ?? host.querySelector(".week-label"))?.focus();
  });

  render();
}

function daysOf(week, byDate) {
  const days = [];
  for (let i = 0; i < 7; i++) {
    const day = addDays(week, i);
    if (i < 5 || byDate.has(isoDate(day))) days.push(day);
  }
  return days;
}

function defaultDay(days, todayIso, byDate) {
  const isos = days.map(isoDate);
  return isos.find((iso) => iso === todayIso)
    ?? isos.find((iso) => iso > todayIso && byDate.has(iso))
    ?? isos.find((iso) => byDate.has(iso))
    ?? isos[0];
}

function navHtml(week, thisWeek, minWeek, maxWeek) {
  const offset = Math.round((week - thisWeek) / WEEK_MS);
  const label = { 0: "Cette semaine", 1: "Semaine prochaine", [-1]: "Semaine dernière" }[offset]
    ?? `Semaine du ${dayMonth(week)}`;
  const friday = addDays(week, 4);
  const range = week.getMonth() === friday.getMonth()
    ? `Du ${week.getDate()} au ${dayMonth(friday)}`
    : `Du ${dayMonth(week)} au ${dayMonth(friday)}`;

  return `
    <div class="week-nav">
      <div class="week-heading">
        <h2 class="week-label" tabindex="-1">${label}</h2>
        <p class="week-range">${range}</p>
      </div>
      ${offset !== 0 ? `<button class="button button-small" type="button" data-action="today">Aujourd'hui</button>` : ""}
      <button class="icon-button" type="button" data-action="prev" aria-label="Semaine précédente" ${week.getTime() <= minWeek ? "disabled" : ""}>${CHEVRON_LEFT}</button>
      <button class="icon-button" type="button" data-action="next" aria-label="Semaine suivante" ${week.getTime() >= maxWeek ? "disabled" : ""}>${CHEVRON_RIGHT}</button>
    </div>`;
}

function agendaHtml(days, selected, byDate, todayIso, mode) {
  const chips = days.map((day) => {
    const iso = isoDate(day);
    const list = byDate.get(iso) ?? [];
    return `
      <button type="button" class="day-chip ${iso === todayIso ? "is-today" : ""}" data-action="day" data-date="${iso}"
        aria-pressed="${iso === selected}" aria-label="${weekdayLong(day)} ${dayMonth(day)}, ${list.length} cours">
        <span class="dname">${weekdayShort(day)}</span>
        <span class="dnum">${day.getDate()}</span>
        <span class="dots" aria-hidden="true">${list.slice(0, 6).map((c) => `<i class="kind-${c.kind}"></i>`).join("")}</span>
      </button>`;
  }).join("");

  const date = parseDate(selected);
  const list = byDate.get(selected) ?? [];

  return `
    <div class="agenda">
      <div class="day-strip" role="group" aria-label="Jours de la semaine">${chips}</div>
      <h3 class="day-title">${weekdayLong(date)} ${dayMonth(date)}</h3>
      ${list.length ? `<ol class="agenda-list">${agendaItems(list, mode)}</ol>` : emptyDay(selected, todayIso)}
    </div>`;
}

function agendaItems(list, mode) {
  let html = "";
  let lastEnd = null;
  for (const course of list) {
    const start = minutes(course.start);
    if (lastEnd !== null && start - lastEnd >= 30) {
      html += `<li class="gap">Pause de ${duration(start - lastEnd)}</li>`;
    }
    html += courseItem(course, mode);
    lastEnd = Math.max(lastEnd ?? 0, minutes(course.end));
  }
  return html;
}

function courseItem(course, mode) {
  const meta = [course.code, course.teacher].filter(Boolean).map(esc).join(", ");
  const room = course.room || "Salle non précisée";
  const where = mode === "group"
    ? `<p class="course-where ${isLongRoom(room) ? "is-long" : ""}">${esc(room)}</p>`
    : "";

  return `
    <li class="course kind-${course.kind}">
      <p class="course-when"><b>${course.start}</b><span>${course.end}</span></p>
      <div>
        <p class="course-title"><span class="tag">${KIND[course.kind]}</span>${esc(course.name)}</p>
        ${meta ? `<p class="course-meta">${meta}</p>` : ""}
      </div>
      ${where}
    </li>`;
}

function emptyDay(iso, todayIso) {
  const text = iso < todayIso
    ? "Pas de données pour ce jour : la source ne donne que les jours à venir. Le site garde ensuite ce qu'il a vu."
    : "Pas de cours ce jour-là.";
  return `<p class="empty-day">${text}</p>`;
}

function gridHtml(days, byDate, todayIso, mode) {
  const weekCourses = days.flatMap((d) => byDate.get(isoDate(d)) ?? []);
  let from = 8;
  let to = 18;
  for (const course of weekCourses) {
    from = Math.min(from, Math.floor(minutes(course.start) / 60));
    to = Math.max(to, Math.ceil(minutes(course.end) / 60));
  }
  const span = to - from;
  const now = nowMinutes();

  const heads = days.map((day) => `
    <div class="wg-dayhead ${isoDate(day) === todayIso ? "is-today" : ""}">
      <span class="wg-dname">${weekdayShort(day)}</span><span class="wg-dnum">${day.getDate()}</span>
    </div>`).join("");

  const hours = Array.from({ length: span }, (_, i) =>
    `<span class="wg-hour" style="--top:${i}">${from + i}h</span>`).join("");

  const columns = days.map((day) => {
    const iso = isoDate(day);
    const list = byDate.get(iso) ?? [];
    const past = iso < todayIso;
    const blocks = layoutDay(list).map((item) => blockHtml(item, from, mode)).join("");
    const nowLine = iso === todayIso && now >= from * 60 && now <= to * 60
      ? `<div class="now-line" style="--top:${(now - from * 60) / 60}" aria-hidden="true"></div>`
      : "";
    const empty = list.length ? "" : `<p class="wg-empty">${past ? "Pas de données" : "Pas de cours"}</p>`;
    return `<div class="wg-day ${past ? "is-past" : ""}">${blocks}${nowLine}${empty}</div>`;
  }).join("");

  return `
    <div class="week-grid" style="--days:${days.length};--span:${span}">
      <div class="wg-corner"></div>${heads}
      <div class="wg-hours" aria-hidden="true">${hours}</div>${columns}
    </div>`;
}

function layoutDay(list) {
  const items = list
    .map((c) => ({ c, s: minutes(c.start), e: minutes(c.end), lane: 0, lanes: 1 }))
    .sort((a, b) => a.s - b.s || b.e - a.e);

  let cluster = [];
  let laneEnds = [];
  let clusterEnd = -1;
  const flush = () => {
    for (const item of cluster) item.lanes = laneEnds.length;
    cluster = [];
    laneEnds = [];
  };

  for (const item of items) {
    if (item.s >= clusterEnd) flush();
    let lane = laneEnds.findIndex((end) => end <= item.s);
    if (lane === -1) {
      lane = laneEnds.length;
      laneEnds.push(item.e);
    } else {
      laneEnds[lane] = item.e;
    }
    item.lane = lane;
    cluster.push(item);
    clusterEnd = Math.max(clusterEnd, item.e);
  }
  flush();
  return items;
}

function blockHtml({ c, s, e, lane, lanes }, from, mode) {
  const label = [
    `${KIND[c.kind]} ${c.name}`,
    `${c.start} à ${c.end}`,
    c.room && `salle ${c.room}`,
    c.teacher,
  ].filter(Boolean).join(", ");

  return `
    <article class="block kind-${c.kind}" style="--top:${(s - from * 60) / 60};--len:${(e - s) / 60};--lane:${lane};--lanes:${lanes}"
      aria-label="${esc(label)}" title="${esc(label)}">
      <p class="block-head">
        ${mode === "group" ? `<span class="block-room">${esc(c.room || "?")}</span>` : ""}
        <span class="block-time">${c.start}–${c.end}</span>
      </p>
      <p class="block-title"><span class="tag">${KIND[c.kind]}</span>${esc(c.name)}</p>
      ${c.teacher ? `<p class="block-who">${esc(c.teacher)}</p>` : ""}
    </article>`;
}
