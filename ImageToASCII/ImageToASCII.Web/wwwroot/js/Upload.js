const uploadForm = document.querySelector("#uploadForm");
const resultStatus = document.querySelector("#resultStatus");
const resultImage = document.querySelector("#resultImage");
const resultVideo = document.querySelector("#resultVideo");
const downloadContainer = document.querySelector("#downloadContainer");
const downloadBtn = document.querySelector("#downloadBtn");

uploadForm.addEventListener("submit", async function (e) {
    e.preventDefault();
    const formData = new FormData(this);

    resultStatus.className = "visible pending";
    resultStatus.textContent = "Обработка файла...";
    resultImage.style.display = "none";
    resultVideo.style.display = "none";
    downloadContainer.style.display = "none";

    try {
        const response = await fetch("/media/convert", {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            const blob = await response.blob();
            const objectURL = URL.createObjectURL(blob);
            const isVideo = blob.type.startsWith("video/");

            if (isVideo) {
                resultVideo.src = objectURL;
                resultVideo.style.display = "block";
            } else {
                resultImage.src = objectURL;
                resultImage.style.display = "block";
            }

            let fileName = isVideo ? "ascii_result.mp4" : "ascii_result.png";
            const disposition = response.headers.get("Content-Disposition");
            if (disposition && disposition.includes("filename=")) {
                fileName = disposition.split("filename=")[1].replace(/["']/g, "").trim();
            }

            downloadBtn.href = objectURL;
            downloadBtn.download = fileName;
            downloadContainer.style.display = "block";

            resultStatus.className = "visible ok";
            resultStatus.textContent = "Успешно обработано!";
        } else {
            const errorText = await response.text();
            resultStatus.className = "visible err";
            resultStatus.textContent = `Ошибка сервера: статус ${response.status}${errorText ? " — " + errorText : ""}`;
        }
    } catch (error) {
        console.error(error);
        resultStatus.className = "visible err";
        resultStatus.textContent = `Ошибка сети: ${error.message}`;
    }
});