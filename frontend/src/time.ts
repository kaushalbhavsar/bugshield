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

export function outcomeLabel(outcome: string): string {
  if (outcome === "disrupted") return "Confirmed disruptive";
  if (outcome === "detected") return "Detected";
  if (outcome === "observed") return "Observed";
  return outcome;
}
