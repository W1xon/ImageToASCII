const uploadForm = document.querySelector("#uploadForm");
const resultStatus = document.querySelector("#resultStatus");
const resultImage = document.querySelector("#resultImage");

uploadForm.addEventListener("submit", async function (e) {
    e.preventDefault();
    const formData = new FormData(this);

    resultStatus.className = "visible pending";
    resultStatus.textContent = "Обработка изображения...";
    resultImage.style.display = "none";

    try {
        const response = await fetch("/api/image/save", {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            const imageBlob = await response.blob();
            const imageObjectURL = URL.createObjectURL(imageBlob);

            resultImage.src = imageObjectURL;
            resultImage.style.display = "block";

            resultStatus.className = "visible ok";
            resultStatus.textContent = "Успешно обработано!";
        } else {
            const errorText = await response.text();
            resultStatus.className = "visible err";
            resultStatus.textContent = `Ошибка сервера: статус ${response.status}${errorText ? " — " + errorText : ""}`;
        }
    } catch (error) {
        resultStatus.className = "visible err";
        resultStatus.textContent = "Ошибка сети: не удалось отправить запрос";
    }
});