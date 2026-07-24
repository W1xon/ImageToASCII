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
const submitBtn = uploadForm ? uploadForm.querySelector("button[type='submit']") : null;

let sourcePreviewURL = null;

window.addEventListener("DOMContentLoaded", () => {
    const savedJobId = localStorage.getItem("activeJobId");
    if (savedJobId) {
        SetSubmitDisabled(true);
        PollJobStatus(savedJobId);
    }
});

InitEvents();

function InitEvents() {
    if (fileInput) fileInput.addEventListener("change", OnFileInputChange);
    if (uploadForm) uploadForm.addEventListener("submit", OnFormSubmit);
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
    if (e) {
        e.preventDefault();
        e.stopPropagation();
    }

    const formData = new FormData(uploadForm);

    ResetResultState();
    SetStatus("pending", "Загрузка файла...");
    SetSubmitDisabled(true);

    try {
        const response = await fetch("/media/convert", {
            method: "POST",
            body: formData
        });

        if (!response.ok) {
            const errorText = await response.text();
            SetStatus("err", `Ошибка сервера: ${response.status} ${errorText}`);
            SetSubmitDisabled(false);
            return;
        }

        const { jobId } = await response.json();
        localStorage.setItem("activeJobId", jobId);

        await PollJobStatus(jobId);

    } catch (error) {
        console.error("Fetch error:", error);
        SetStatus("err", `Ошибка передачи: ${error.message}`);
        SetSubmitDisabled(false);
    }
}

async function PollJobStatus(jobId) {
    const delayMs = 1000;
    let isCompleted = false;
    let failCount = 0;

    while (!isCompleted) {
        try {
            const response = await fetch(`/media/status/${jobId}?_=${Date.now()}`, {
                cache: "no-store"
            });

            if (!response.ok) {
                SetStatus("err", "Не удалось получить статус задачи");
                break;
            }

            const { status } = await response.json();
            failCount = 0;

            if (status === "Pending") {
                SetStatus("pending", "В очереди на обработку...");
            } else if (status === "Processing") {
                SetStatus("pending", "Обработка медиа...");
            } else if (status === "Completed") {
                SetStatus("pending", "Получение результата...");
                isCompleted = true;
            } else if (status === "Failed") {
                SetStatus("err", "Ошибка при обработке файла на сервере");
                break;
            }

            if (!isCompleted) {
                await new Promise(resolve => setTimeout(resolve, delayMs));
            }

        } catch (error) {
            failCount++;
            console.warn(`Polling error (${failCount}/3):`, error);

            if (failCount >= 3) {
                SetStatus("err", "Связь с сервером потеряна");
                break;
            }

            await new Promise(resolve => setTimeout(resolve, delayMs));
        }
    }

    localStorage.removeItem("activeJobId");

    if (isCompleted) {
        await FetchResult(jobId);
    } else {
        SetSubmitDisabled(false);
    }
}

async function FetchResult(jobId) {
    try {
        const resultUrl = `/media/result/${jobId}`;

        const response = await fetch(resultUrl, { method: "HEAD" });

        if (!response.ok) {
            SetStatus("err", "Ошибка при скачивании результата");
            SetSubmitDisabled(false);
            return;
        }

        const contentType = response.headers.get("Content-Type") || "";
        const isVideo = contentType.startsWith("video/");
        const fileName = ExtractFileName(response, isVideo);

        RenderResult(resultUrl, isVideo, fileName);
        SetStatus("ok", "Успешно обработано!");
    } catch (error) {
        console.error("FetchResult error:", error);
        SetStatus("err", "Ошибка при получении итогового файла");
    } finally {
        SetSubmitDisabled(false);
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
        resultVideo.load();
    } else {
        resultImage.src = url;
        resultImage.style.display = "block";
    }

    downloadBtn.href = url;
    downloadBtn.download = fileName;
    downloadContainer.style.display = "block";
}

function SetStatus(type, message) {
    if (!resultStatus) return;
    resultStatus.className = `visible ${type}`;
    resultStatus.textContent = message;
}

function SetSubmitDisabled(disabled) {
    if (submitBtn) {
        submitBtn.disabled = disabled;
        submitBtn.style.opacity = disabled ? "0.6" : "1";
        submitBtn.style.cursor = disabled ? "not-allowed" : "pointer";
    }
}

function ClearSourcePreview() {
    if (sourcePreviewURL) {
        URL.revokeObjectURL(sourcePreviewURL);
        sourcePreviewURL = null;
    }
    if (filePreview) filePreview.innerHTML = "";
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