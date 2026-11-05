import {
  KIND, api, esc, storage, minutes, nowMinutes, parseDate, isoDate, daysBetween,
  weekdayLong, dayMonth, sinceLabel, dateTimeLabel, isLongRoom,
} from "./util.js";
import { mountWeek } from "./week.js";

const view = document.getElementById("view");
const title = document.getElementById("title");
const subtitle = document.getElementById("subtitle");

const ROOMS_AUTO_LIMIT = 60;

let campusesCache = null;
const campuses = async () => (campusesCache ??= await api("/campus"));

function setHeader(heading, sub = "", action = null) {
  title.textContent = heading;
  subtitle.innerHTML = esc(sub) + (action ? ` <a href="${action.href}">${esc(action.label)}</a>` : "");
  document.title = heading ? `${heading}, Où j'ai cours` : "Où j'ai cours";
}

export function setTab(tab) {
  for (const link of document.querySelectorAll(".tab")) {
    if (link.dataset.tab === tab) link.setAttribute("aria-current", "page");
    else link.removeAttribute("aria-current");
  }
}

const loading = (text) => `<p class="loading" role="status">${esc(text)}</p>`;

export function showProblem(error, retry) {
  const unavailable = error.status === 503;
  const missing = error.status === 404;
  const heading = unavailable ? "La source ne répond pas" : missing ? "Introuvable" : "Ça n'a pas marché";
  const text = unavailable
    ? "Le script qui lit ADE ne répond pas pour le moment. Réessaie dans quelques minutes."
    : missing
      ? "Ce groupe ou cette salle n'existe pas sur le site."
      : error.message;

  view.innerHTML = `
    <div class="problem" role="alert">
      <h2 class="problem-title">${heading}</h2>
      <p>${esc(text)}</p>
      <div class="problem-actions">
        ${missing ? "" : `<button class="button button-primary" type="button" id="retry">Réessayer</button>`}
        <a class="button" href="#/choisir">Choisir mon groupe</a>
      </div>
    </div>`;
  view.querySelector("#retry")?.addEventListener("click", retry);
}

function freshness(fetchedAt, isStale) {
  return isStale
    ? `<p class="notice">La source ne répond pas. Tu vois la version du ${dateTimeLabel(fetchedAt)}.</p>`
    : `<p class="freshness">Mis à jour ${sinceLabel(fetchedAt)}</p>`;
}

function plaque({ when, room, heading, meta, then, label }) {
  return `
    <section class="plaque" aria-label="${esc(label)}">
      <p class="plaque-when">${esc(when)}</p>
      <p class="plaque-room ${isLongRoom(room) ? "is-long" : ""}">${esc(room)}</p>
      ${heading ? `<p class="plaque-title">${esc(heading)}</p>` : ""}
      ${meta ? `<p class="plaque-meta">${esc(meta)}</p>` : ""}
      ${then ? `<p class="plaque-then">${esc(then)}</p>` : ""}
    </section>`;
}

function whenLabel(course, now) {
  const days = daysBetween(now, parseDate(course.date));
  if (days === 0) {
    const wait = minutes(course.start) - nowMinutes(now);
    return wait <= 60 ? `Prochain cours dans ${wait} min` : `Prochain cours aujourd'hui à ${course.start}`;
  }
  if (days === 1) return `Prochain cours demain à ${course.start}`;
  if (days < 7) return `Prochain cours ${weekdayLong(parseDate(course.date)).toLowerCase()} à ${course.start}`;
  return `Prochain cours le ${dayMonth(parseDate(course.date))} à ${course.start}`;
}

function groupPlaque(courses) {
  const now = new Date();
  const today = isoDate(now);
  const time = nowMinutes(now);

  const ongoing = courses
    .filter((c) => c.date === today && minutes(c.start) <= time && time < minutes(c.end))
    .sort((a, b) => minutes(b.end) - minutes(a.end))[0];
  const after = ongoing ? minutes(ongoing.end) : time;
  const next = courses.find((c) => c.date > today || (c.date === today && minutes(c.start) >= after && c !== ongoing));

  const describe = (c) => `${KIND[c.kind]}, ${c.start} à ${c.end}${c.teacher ? `, ${c.teacher}` : ""}`;

  if (ongoing) {
    const then = next
      ? `Ensuite ${next.date === today ? `à ${next.start}` : whenLabel(next, now).replace("Prochain cours ", "")}, salle ${next.room || "non précisée"}`
      : "Plus rien ensuite.";
    return plaque({
      label: "Cours en cours",
      when: `En cours, jusqu'à ${ongoing.end}`,
      room: ongoing.room || "?",
      heading: ongoing.name,
      meta: describe(ongoing),
      then,
    });
  }
  if (next) {
    return plaque({
      label: "Prochain cours",
      when: whenLabel(next, now),
      room: next.room || "?",
      heading: next.name,
      meta: describe(next),
    });
  }
  return plaque({
    label: "Prochain cours",
    when: "Rien de prévu",
    room: "Libre",
    heading: "Aucun cours dans les semaines à venir.",
  });
}

function roomPlaque(room, courses) {
  const now = new Date();
  const today = isoDate(now);
  const time = nowMinutes(now);
  const todays = courses.filter((c) => c.date === today);
  const current = todays.filter((c) => minutes(c.start) <= time && time < minutes(c.end))
    .sort((a, b) => minutes(b.end) - minutes(a.end))[0];
  const next = todays.find((c) => minutes(c.start) > time);

  if (current) {
    return plaque({
      label: "État de la salle",
      when: `Occupée jusqu'à ${current.end}`,
      room: room.label,
      heading: current.name,
      meta: [KIND[current.kind], current.teacher].filter(Boolean).join(", "),
    });
  }
  return plaque({
    label: "État de la salle",
    when: next ? `Libre jusqu'à ${next.start}` : "Libre jusqu'à ce soir",
    room: room.label,
    heading: next ? `Ensuite : ${next.name}` : "Aucun autre cours aujourd'hui.",
  });
}

export async function groupView(resourceId, isCurrent) {
  const saved = storage.get("group");
  if (saved?.id === resourceId) setHeader(saved.title, saved.sub, { href: "#/choisir", label: "Changer de groupe" });
  else setHeader("Ton emploi du temps");
  view.innerHTML = loading("Chargement de ton emploi du temps…");

  let data;
  try {
    data = await api(`/group/${resourceId}/timetable`);
  } catch (error) {
    if (isCurrent()) showProblem(error, () => groupView(resourceId, isCurrent));
    return;
  }
  if (!isCurrent()) return;

  const group = data.group;
  const sub = `${group.yearLabel}, ${group.label}`;
  storage.set("group", { id: resourceId, campusId: group.campusId, title: group.formationName, sub });
  setHeader(group.formationName, sub, { href: "#/choisir", label: "Changer de groupe" });

  const calendarUrl = `${location.origin}/api/v1/group/${resourceId}/calendar.ics`;
  view.innerHTML = `
    <div class="board">
      <aside class="board-aside">
        ${groupPlaque(data.courses)}
        ${freshness(data.fetchedAt, data.isStale)}
        <div class="subscribe">
          <button class="button" type="button" id="copy-calendar">Copier le lien d'agenda</button>
          <details class="howto">
            <summary>Comment l'ajouter à mon agenda ?</summary>
            <p>Google Agenda : dans « Autres agendas », choisis « À partir de l'URL » et colle le lien.</p>
            <p>iPhone : Réglages, Calendrier, Comptes, Ajouter un compte, Autre, puis « Ajouter un calendrier avec abonnement ».</p>
            <p>Ton agenda se met ensuite à jour tout seul.</p>
          </details>
        </div>
      </aside>
      <section class="board-main" id="week" aria-label="Semaine"></section>
    </div>`;

  mountWeek(view.querySelector("#week"), { courses: data.courses, mode: "group" });

  const copy = view.querySelector("#copy-calendar");
  copy.addEventListener("click", async () => {
    try {
      await navigator.clipboard.writeText(calendarUrl);
      copy.textContent = "Lien d'agenda copié";
      setTimeout(() => (copy.textContent = "Copier le lien d'agenda"), 2500);
    } catch {
      copy.insertAdjacentHTML("afterend", `<input readonly value="${esc(calendarUrl)}" aria-label="Lien d'agenda">`);
      copy.nextElementSibling.select();
    }
  });
}

export async function pickerView(isCurrent) {
  setHeader("Trouve ton emploi du temps", "Choisis ton campus, ta formation, ton année et ton groupe. Le site s'en souviendra.");
  view.innerHTML = loading("Chargement des campus…");

  let list;
  try {
    list = await campuses();
  } catch (error) {
    if (isCurrent()) showProblem(error, () => pickerView(isCurrent));
    return;
  }
  if (!isCurrent()) return;

  const withFormations = list.filter((c) => c.formationCount > 0);
  const saved = storage.get("group");
  const state = { campus: null, details: null, formation: null, year: null };

  const choose = async (campusId) => {
    state.campus = campusId;
    state.formation = null;
    state.year = null;
    state.details = null;
    render();
    try {
      state.details = await api(`/campus/${campusId}`);
    } catch (error) {
      showProblem(error, () => pickerView(isCurrent));
      return;
    }
    if (state.details.formations.length === 1) state.formation = state.details.formations[0].id;
    render();
  };

  const step = (n, heading, status, body) => `
    <section class="step is-${status}">
      <span class="step-num" aria-hidden="true">${n}</span>
      <div><h2 class="step-title">${heading}</h2>${body}</div>
    </section>`;
  const locked = (text) => `<p class="step-note">${text}</p>`;
  const choice = (attrs, pressed, label, hint = "") =>
    `<button type="button" class="choice" ${attrs} aria-pressed="${pressed}">${esc(label)}${hint ? `<small>${esc(hint)}</small>` : ""}</button>`;

  function render() {
    const formation = state.details?.formations.find((f) => f.id === state.formation);
    const year = formation?.years.find((y) => y.year === state.year);

    const campusStep = step(1, "Campus", state.campus ? "done" : "current", `
      <div class="choices">
        ${withFormations.map((c) => choice(`data-campus="${c.id}"`, c.id === state.campus, c.name,
          `${c.formationCount} formation${c.formationCount > 1 ? "s" : ""}`)).join("")}
      </div>
      <p class="step-note">Les autres campus n'ont pas encore de formation sur le site. Leurs salles sont déjà dans <a href="#/salles">Salles libres</a>.</p>`);

    const formationStep = !state.details
      ? step(2, "Formation", "locked", locked(state.campus ? "Chargement…" : "Choisis d'abord ton campus."))
      : step(2, "Formation", formation ? "done" : "current", `
          <div class="choices">
            ${state.details.formations.map((f) => choice(`data-formation="${esc(f.id)}"`, f.id === state.formation, f.name)).join("")}
          </div>`);

    const yearStep = !formation
      ? step(3, "Année", "locked", locked("Choisis d'abord ta formation."))
      : step(3, "Année", year ? "done" : "current", `
          <div class="choices">
            ${formation.years.map((y) => choice(`data-year="${y.year}"`, y.year === state.year, y.label)).join("")}
          </div>`);

    let groupStep;
    if (!year) {
      groupStep = step(4, "Groupe", "locked", locked("Choisis d'abord ton année."));
    } else {
      const byParent = Map.groupBy(year.groups, (g) => g.parent);
      groupStep = step(4, "Groupe", "current", [...byParent].map(([parent, groups]) => `
        <div class="td-row">
          <span class="td-label">${esc(parent)}</span>
          <div class="choices">
            ${groups.map((g) => choice(`data-group="${g.resourceId}"`, g.resourceId === saved?.id, g.label)).join("")}
          </div>
        </div>`).join(""));
    }

    view.innerHTML = `<div class="picker">${campusStep}${formationStep}${yearStep}${groupStep}</div>`;
  }

  view.onclick = (event) => {
    const button = event.target.closest(".choice");
    if (!button) return;
    const { campus, formation, year, group } = button.dataset;
    if (campus) choose(Number(campus));
    if (formation) {
      state.formation = formation;
      state.year = null;
      render();
    }
    if (year) {
      state.year = Number(year);
      render();
    }
    if (group) location.hash = `#/groupe/${group}`;
  };

  render();
  const start = saved?.campusId ?? (withFormations.length === 1 ? withFormations[0].id : null);
  if (start && withFormations.some((c) => c.id === start)) choose(start);
}

export async function roomsView(campusId, isCurrent) {
  setHeader("Salles libres", "Une salle pour bosser ? Voici ce qui est libre maintenant.");
  view.innerHTML = loading("Chargement des campus…");

  let list;
  try {
    list = await campuses();
  } catch (error) {
    if (isCurrent()) showProblem(error, () => roomsView(campusId, isCurrent));
    return;
  }
  if (!isCurrent()) return;

  const withRooms = list.filter((c) => c.roomCount > 0);
  const campus = withRooms.find((c) => c.id === campusId)
    ?? withRooms.find((c) => c.id === storage.get("group")?.campusId)
    ?? withRooms.find((c) => c.formationCount > 0)
    ?? withRooms[0];

  view.innerHTML = `
    <div class="rooms-head">
      <select class="field field-select" id="campus-select" aria-label="Campus">
        ${withRooms.map((c) => `<option value="${c.id}" ${c.id === campus.id ? "selected" : ""}>${esc(c.name)} (${c.roomCount} salles)</option>`).join("")}
      </select>
      <input class="field field-search" id="room-search" type="search" placeholder="Chercher une salle" aria-label="Chercher une salle" autocomplete="off">
    </div>
    <div id="rooms-body"></div>`;

  view.querySelector("#campus-select").addEventListener("change", (event) => {
    location.hash = `#/salles/${event.target.value}`;
  });

  const body = view.querySelector("#rooms-body");
  const load = async () => {
    body.innerHTML = loading(`Lecture du planning de ${campus.roomCount} salles…`);
    try {
      const status = await api(`/room/status?campusId=${campus.id}`);
      if (isCurrent()) renderRooms(body, status);
    } catch (error) {
      if (isCurrent()) showProblem(error, () => roomsView(campus.id, isCurrent));
    }
  };

  if (campus.roomCount <= ROOMS_AUTO_LIMIT) {
    load();
  } else {
    body.innerHTML = `
      <div class="problem">
        <h2 class="problem-title">${campus.roomCount} salles sur ce campus</h2>
        <p>Pour savoir lesquelles sont libres, le site lit le planning de chacune. La première fois, ça peut prendre une minute.</p>
        <div class="problem-actions"><button class="button button-primary" type="button" id="load-rooms">Voir les salles libres</button></div>
      </div>`;
    body.querySelector("#load-rooms").addEventListener("click", load);
  }

  view.querySelector("#room-search").addEventListener("input", (event) => {
    const query = event.target.value.trim().toLowerCase();
    for (const row of body.querySelectorAll("[data-name]")) {
      row.hidden = query !== "" && !row.dataset.name.includes(query);
    }
    for (const section of body.querySelectorAll("[data-section]")) {
      section.hidden = !section.querySelector("[data-name]:not([hidden])");
    }
  });
}

function renderRooms(body, status) {
  const free = status.rooms.filter((r) => r.state === "free")
    .sort((a, b) => (b.freeUntil ?? "99") .localeCompare(a.freeUntil ?? "99") || a.room.label.localeCompare(b.room.label, "fr", { numeric: true }));
  const busy = status.rooms.filter((r) => r.state === "busy")
    .sort((a, b) => a.busyUntil.localeCompare(b.busyUntil) || a.room.label.localeCompare(b.room.label, "fr", { numeric: true }));
  const unknown = status.rooms.filter((r) => r.state === "unknown");

  const row = (r) => {
    const label = r.room.label;
    const state = r.state === "free"
      ? (r.freeUntil ? `Libre jusqu'à ${r.freeUntil}` : "Libre jusqu'à ce soir")
      : r.state === "busy" ? `Occupée jusqu'à ${r.busyUntil}` : "État inconnu";
    const detail = r.state === "free"
      ? (r.next ? `Ensuite : ${r.next.name}` : "Plus de cours aujourd'hui")
      : r.state === "busy" ? r.current.name : "Planning indisponible pour le moment";
    return `
      <li data-name="${esc(label.toLowerCase())}">
        <a class="room-row is-${r.state}" href="#/salle/${r.room.resourceId}">
          <span class="room-name ${isLongRoom(label) ? "is-long" : ""}">${esc(label)}</span>
          <span class="room-state">${esc(state)}</span>
          <span class="room-detail">${esc(detail)}</span>
        </a>
      </li>`;
  };

  const section = (heading, rooms) => rooms.length ? `
    <section data-section>
      <h2 class="room-section-title">${heading} (${rooms.length})</h2>
      <ul class="room-list">${rooms.map(row).join("")}</ul>
    </section>` : "";

  body.innerHTML = `
    <p class="rooms-summary">État à ${status.time}, ${weekdayLong(parseDate(status.date)).toLowerCase()} ${dayMonth(parseDate(status.date))}.</p>
    ${section("Libres maintenant", free)}
    ${section("Occupées", busy)}
    ${section("État inconnu", unknown)}`;
}

export async function roomView(resourceId, isCurrent) {
  setHeader("Salle");
  view.innerHTML = loading("Chargement du planning de la salle…");

  let data;
  try {
    data = await api(`/room/${resourceId}/timetable`);
  } catch (error) {
    if (isCurrent()) showProblem(error, () => roomView(resourceId, isCurrent));
    return;
  }
  if (!isCurrent()) return;

  setHeader(data.room.label, data.campusName, { href: `#/salles/${data.room.campusId}`, label: "Toutes les salles" });
  view.innerHTML = `
    <div class="board">
      <aside class="board-aside">
        ${roomPlaque(data.room, data.courses)}
        ${freshness(data.fetchedAt, data.isStale)}
      </aside>
      <section class="board-main" id="week" aria-label="Semaine"></section>
    </div>`;
  mountWeek(view.querySelector("#week"), { courses: data.courses, mode: "room" });
}
