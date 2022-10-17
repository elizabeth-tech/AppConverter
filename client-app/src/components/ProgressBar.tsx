import "./progress-bar.css";

export type ProgressBarProps = {
  value: number;
  max: number;
};

export function ProgressBar({ value, max }: ProgressBarProps) {
  return <div className="progress"><p>Загрузка...</p> <progress value={value} max={max} /></div>;
}
