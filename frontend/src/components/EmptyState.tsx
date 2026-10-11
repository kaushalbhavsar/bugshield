import type { ReactNode } from "react";

export function EmptyState({
  title,
  children,
}: {
  title: string;
  children: ReactNode;
}) {
  return (
    <div className="card empty">
      <div className="card-content">
        <h3 className="title">{title}</h3>
        <p className="category">{children}</p>
      </div>
    </div>
  );
}
