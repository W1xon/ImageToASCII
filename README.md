<div align="center">

# 🖼️ ImageToASCII

*Конвертер картинок и видео в ASCII-арт. Консоль или браузер — на выбор.*

<img src="Assets/Placeholder.png" width="100%" alt="ImageToASCII Logo">

</div>

---

## 💀 Обзор

**ImageToASCII** превращает изображения и видео в ASCII-арт.

С версии 3.0 проект состоит из трёх частей:

- **CLI** — консольная утилита, без лимитов, работает локально
- **Web** — сервис на ASP.NET Core, можно попробовать прямо в браузере
- **Core** — общая библиотека конвертации, которую используют и CLI, и Web

---

## 🧠 Что умеет

- 🖼️ Картинки → ASCII (PNG)
- 🎞️ Видео → ASCII-видео, со звуком
- 🌐 Веб-версия — загрузка файла, очередь на обработку, скачивание результата
- 💻 CLI — та же логика, без лимитов веб-версии
- 🎨 4 цветовые палитры (Basic, Palette16, Palette7, Natural), 10 наборов символов
- 🐧 Linux и Windows

---

## ⚡ v3.0 — что изменилось

Крупное обновление проекта.

- Логика конвертации вынесена в отдельную библиотеку `ImageToASCII.Core`.
- Появилась веб-версия: drag&drop загрузка, статус обработки в реальном времени, очередь заданий.
- Проверка файлов по сигнатуре, а не по расширению.
- Ускорение конвертации видео

---

## 🌐 Сайт vs 💻 CLI

Сайт — для быстрой проверки без установки.

CLI — без лимитов, работает локально, полный контроль над настройками.

🌐 Попробовать: чутка позже
💻 Скачать: [релизы на GitHub](https://github.com/W1xon/ImageToASCII/releases)

---

## 🗺️ Версии

- **v3.0** — Core вынесен отдельно, добавлена веб-версия, движок ускорен
- **v2.1** — склейка звука с видео, поддержка Linux
- **v2.0** — конвертация видео в ASCII
- **v1.0** — конвертация изображений, консоль

---

## 📸 Скриншоты

### 🧩 Консоль

<div align="center">
  <img src="Assets/Image_Console.png" width="80%" alt="Консольное меню">
</div>

### 🖼️ Изображения → ASCII

<div align="center">
  <img src="Assets/Image1.png" width="30%" alt="Оригинал">
  <img src="Assets/Image1_ASCII.png" width="30%" alt="ASCII">
  <img src="Assets/Image2.jpg" width="30%" alt="Оригинал">
</div>
<div align="center">
  <img src="Assets/Image2_ASCII.png" width="30%" alt="ASCII">
  <img src="Assets/Image3.jpg" width="30%" alt="Оригинал">
  <img src="Assets/Image3_ASCII.png" width="30%" alt="ASCII">
</div>

### 🌐 Веб-версия

<div align="center">
  <img src="Assets/Web_Compare.png" width="45%" alt="Основная страница">
  <img src="Assets/Web_Upload.png" width="45%" alt="Страница загрузки">
</div>

---

## 🎞️ Видео → ASCII

<div align="center">
<video src="https://github.com/W1xon/ImageToASCII/blob/main/Assets/Video_ASCII.webm" width="50%" controls></video>
<p><sub>
Каждый кадр обработан, звук склеен с оригиналом — теперь еще быстрее.
</sub></p>
</div>

---

## 👨‍💻 Автор

Создан **Wixon Shade** — человеком, который посмотрел на видео и сказал: «А что если… в ASCII для всех?». Потом переписал всё с нуля.

---

## ☕ Поддержка

Хочешь больше странных экспериментов, которые внезапно становятся рабочими инструментами?

→ [💰 Donation Alerts](https://dalink.to/w1xon)

Где я живу и иногда объясняю, **зачем всё это**:

- 📢 Telegram — https://t.me/CoderW0rker
- 🎥 YouTube — https://www.youtube.com/@w_ixon

> Каждая кружка кофе повышает шанс, что следующая версия будет ещё опаснее.

---

<div align="center">
  <sub>© 2026 Wixon Shade — Сделано с 🖤, C#, и чрезмерным вниманием к ASCII.</sub><br>
  <img src="Assets/Logo.png" width="25%" alt="Footer Logo">
</div>