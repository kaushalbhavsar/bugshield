export function formatTime(value: string, timeZone: string): string {
  try {
    return new Intl.DateTimeFormat(undefined, {
      timeZone,
      year: "numeric",
      month: "short",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
    }).format(new Date(value));
  } catch {
    return value;
  }
}

export function relativeTime(value: string): string {
  const then = new Date(value).getTime();
  const delta = Date.now() - then;
  const minutes = Math.max(0, Math.floor(delta / 60000));
  const hours = Math.floor(minutes / 60);
  const days = Math.floor(hours / 24);
  if (days > 0) return days === 1 ? "1 day ago" : `${days} days ago`;
  if (hours > 0) return hours === 1 ? "1 hour ago" : `${hours} hours ago`;
  return `${minutes} minutes ago`;
}

export function outcomeLabel(outcome: string): string {
  if (outcome === "disrupted") return "Confirmed disruptive";
  if (outcome === "detected") return "Detected";
  if (outcome === "observed") return "Observed";
  return outcome;
}
