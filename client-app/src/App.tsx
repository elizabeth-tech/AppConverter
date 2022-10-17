import axios from "axios";
import { useState } from "react";
import "./App.css";
import { ProgressBar } from "./components/ProgressBar";
import { Spinner } from "./components/Spinner";
import { BACKEND_URI } from "./config/constants";

// ----------------------------------------------------------------------

function App() {
  const [file, setFile] = useState("");
  const [uploadPercentage, setUploadPercentage] = useState(0);
  const [isConverting, setIsConverting] = useState(false);
  const [isLoad, setIsLoad] = useState(false);
  const [testClientGuid, setTestClientGuid] = useState(0);
  var clientGuid = 0;
  var stop = false;

  const onFileChange = (e: any) => {
    console.log(e.target.files[0]);
    if (e.target && e.target.files[0]) {
      setFile(e.target.files[0]);
    }
  };

  const request_retry = () => {
    var i = 0;
    var timerId = setTimeout(function retry() {
      if (i < 12 && !stop) {
        axios({
          url: `${BACKEND_URI}/api/convert/check-file`,
          method: "HEAD",
          params: { guid: clientGuid },
        })
          .then((success) => {
            stop = true;
            setIsConverting(false);
            setIsLoad(true);
          })
          .catch((error) => {
            console.log(error);
          });

        setTimeout(retry, 5000);
      }
      i++;
    }, 5000);

    if (stop) {
      clearTimeout(timerId);
    }
  };

  const handleLoad = (event: { preventDefault: () => void }) => {
    event.preventDefault();
    console.log("client guid", testClientGuid);

    axios({
      url: `${BACKEND_URI}/api/convert/download-file`,
      method: "GET",
      responseType: "blob",
      params: { guid: testClientGuid },
    }).then((response) => {
      // create file link in browser's memory
      const href = URL.createObjectURL(response.data);

      // create "a" HTML element with href to file & click
      const link = document.createElement("a");
      link.href = href;
      link.setAttribute("download", "file.pdf"); //or any other extension
      document.body.appendChild(link);
      link.click();

      // clean up "a" element & remove ObjectURL
      document.body.removeChild(link);
      URL.revokeObjectURL(href);
    });
  };

  const handleSubmit = (event: { preventDefault: () => void }) => {
    event.preventDefault();

    const formData = new FormData();
    formData.append("uploadedHtmlFile", file);

    const options = {
      onUploadProgress: (progressEvent: any) => {
        const { loaded, total } = progressEvent;
        let percent = Math.floor((loaded * 100) / total);
        if (percent < 100) {
          setUploadPercentage(percent);
        }
      },
    };

    axios
      .post(`${BACKEND_URI}/api/convert/html-pdf`, formData, options)
      .then((success) => {
        clientGuid = success.data;
        setTestClientGuid(success.data);
        setUploadPercentage(100);
        setIsConverting(true);
        request_retry();
        setTimeout(() => {
          setUploadPercentage(0);
        }, 1000);
      })
      .catch((error) => {
        setUploadPercentage(0);
        setIsConverting(false);
      });
  };

  return (
    <div className="container">
      <div className="header">Преобразование файлов HTML в PDF</div>

      <div className="block">
        <form onSubmit={handleSubmit}>
          {!isConverting && !isLoad && (
            <>
              <p>Выберите файл расширения .html</p>
              <input
                type="file"
                accept=".html"
                name="uploadedHtmlFile"
                onChange={onFileChange}
              />
            </>
          )}
          {uploadPercentage == 0 && !isConverting && !isLoad && (
            <div className="btn">
              <button>Конвертировать</button>
            </div>
          )}
          {uploadPercentage > 0 && (
            <div className="row mt-3">
              <div className="col pt-1">
                <ProgressBar value={uploadPercentage} max={100} />
              </div>
            </div>
          )}
        </form>
        {uploadPercentage == 0 && isConverting && <Spinner />}
        {uploadPercentage == 0 && !isConverting && isLoad && (
          <div className="btn">
            <button onClick={handleLoad}>Получить файл</button>
          </div>
        )}
      </div>
    </div>
  );
}

export default App;
