import { ClipLoader } from "react-spinners";

export function Spinner() {
  return (
    <div>
      <p>Подождите, идет конвертация файла...</p>
      <ClipLoader
        color="#4c5baf"
        loading={true}
        size={40}
        aria-label="Loading Spinner"
        data-testid="loader"
      />
    </div>
  );
}
