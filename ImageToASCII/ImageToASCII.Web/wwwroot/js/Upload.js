const uploadForm = document.querySelector("#uploadForm");
const fileInput = document.querySelector("#fileInput");
const fileDropContent = document.querySelector("#fileDropContent");
const fileDropName = document.querySelector("#fileDropName");
const filePreview = document.querySelector("#filePreview");

const resultStatus = document.querySelector("#resultStatus");
const resultImage = document.querySelector("#resultImage");
const resultVideo = document.querySelector("#resultVideo");
const resultPlaceholder = document.querySelector("#resultPlaceholder");
const downloadContainer = document.querySelector("#downloadContainer");
const downloadBtn = document.querySelector("#downloadBtn");

let sourcePreviewURL = null;
let resultObjectURL = null;

InitEvents();

function InitEvents() {
    fileInput.addEventListener("change", OnFileInputChange);
    uploadForm.addEventListener("submit", OnFormSubmit);
}

function OnFileInputChange() {
    ClearSourcePreview();

    if (this.files.length > 0) {
        const file = this.files[0];
        fileDropName.textContent = TruncateFileName(file.name);
        fileDropContent.style.display = "none";

        sourcePreviewURL = URL.createObjectURL(file);
        RenderSourcePreview(sourcePreviewURL, file.type.startsWith("video/"));
    } else {
        fileDropName.textContent = "";
        fileDropContent.style.display = "block";
        filePreview.style.display = "none";
    }

    ResetResultState();
}

async function OnFormSubmit(e) {
    e.preventDefault();

    const formData = new FormData(uploadForm);

    ResetResultState();
    SetStatus("pending", "Обработка файла...");

    try {
        const response = await fetch("/media/convert", {
            method: "POST",
            body: formData
        });

        if (!response.ok) {
            const errorText = await response.text();
            SetStatus("err", `Ошибка сервера: ${response.status} ${errorText}`);
            return;
        }

        const blob = await response.blob();

        if (resultObjectURL) {
            URL.revokeObjectURL(resultObjectURL);
        }
        resultObjectURL = URL.createObjectURL(blob);

        const isVideo = blob.type.startsWith("video/");
        const fileName = ExtractFileName(response, isVideo);

        RenderResult(resultObjectURL, isVideo, fileName);
        SetStatus("ok", "Успешно обработано!");

    } catch (error) {
        console.error("Fetch error:", error);
        SetStatus("err", `Ошибка передачи: ${error.message}`);
    }
}

function RenderSourcePreview(url, isVideo) {
    if (isVideo) {
        const video = document.createElement("video");
        video.src = url;
        video.muted = true;
        filePreview.appendChild(video);
    } else {
        const img = document.createElement("img");
        img.src = url;
        filePreview.appendChild(img);
    }
    filePreview.style.display = "flex";
}

function RenderResult(url, isVideo, fileName) {
    if (resultPlaceholder) {
        resultPlaceholder.style.display = "none";
    }

    if (isVideo) {
        resultVideo.src = url;
        resultVideo.style.display = "block";
    } else {
        resultImage.src = url;
        resultImage.style.display = "block";
    }

    downloadBtn.href = url;
    downloadBtn.download = fileName;
    downloadContainer.style.display = "block";
}

function SetStatus(type, message) {
    resultStatus.className = `visible ${type}`;
    resultStatus.textContent = message;
}

function ClearSourcePreview() {
    if (sourcePreviewURL) {
        URL.revokeObjectURL(sourcePreviewURL);
        sourcePreviewURL = null;
    }
    filePreview.innerHTML = "";
}

function ResetResultState() {
    if (resultImage) {
        resultImage.style.display = "none";
        resultImage.src = "";
    }
    if (resultVideo) {
        resultVideo.style.display = "none";
        resultVideo.src = "";
    }
    if (downloadContainer) {
        downloadContainer.style.display = "none";
    }
    if (resultPlaceholder) {
        resultPlaceholder.style.display = "flex";
    }
}

function ExtractFileName(response, isVideo) {
    let fileName = isVideo ? "ascii_result.mp4" : "ascii_result.png";
    const disposition = response.headers.get("Content-Disposition");

    if (disposition && disposition.includes("filename=")) {
        fileName = disposition.split("filename=")[1].replace(/["']/g, "").trim();
    }

    return fileName;
}

function TruncateFileName(name, maxLength = 24) {
    if (name.length <= maxLength) return name;
    const extIndex = name.lastIndexOf('.');
    if (extIndex === -1) return name.slice(0, maxLength - 3) + '...';

    const ext = name.slice(extIndex);
    const avail = maxLength - ext.length - 3;
    const front = Math.ceil(avail / 2);
    const back = Math.floor(avail / 2);

    return name.substring(0, front) + '...' + name.substring(name.length - ext.length - back) + ext;
}