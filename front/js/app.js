import { storage } from "./util.js";
import { groupView, pickerView, roomsView, roomView, setTab } from "./views.js";

const view = document.getElementById("view");
let navigation = 0;

const routes = [
  [/^#?\/?$/, (_, isCurrent) => {
    const saved = storage.get("group");
    return saved ? groupView(saved.id, isCurrent) : pickerView(isCurrent);
  }, "edt"],
  [/^#\/choisir$/, (_, isCurrent) => pickerView(isCurrent), "edt"],
  [/^#\/groupe\/(\d+)$/, (m, isCurrent) => groupView(Number(m[1]), isCurrent), "edt"],
  [/^#\/salles(?:\/(\d+))?$/, (m, isCurrent) => roomsView(m[1] ? Number(m[1]) : null, isCurrent), "salles"],
  [/^#\/salle\/(\d+)$/, (m, isCurrent) => roomView(Number(m[1]), isCurrent), "salles"],
];

function route(firstLoad = false) {
  const token = ++navigation;
  const isCurrent = () => token === navigation;

  view.onclick = null;
  const hash = location.hash || "#/";
  const [pattern, show, tab] = routes.find(([p]) => p.test(hash)) ?? routes[0];
  setTab(tab);
  show(hash.match(pattern), isCurrent);

  if (!firstLoad) {
    window.scrollTo(0, 0);
    view.focus({ preventScroll: true });
  }
}

window.addEventListener("hashchange", () => route());
route(true);
