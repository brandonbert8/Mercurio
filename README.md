<div align="center">

# ☿ Mercurio

### Modern Windows workspace built with C# and WPF

A native Windows application focused on delivering a clean, fast, and intuitive desktop experience through modern UI principles and a scalable architecture.

<p>
  <img src="https://img.shields.io/badge/Status-In_Development-22C55E?style=for-the-badge"/>
  <img src="https://img.shields.io/badge/.NET-9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white"/>
  <img src="https://img.shields.io/badge/WPF-Windows-0A84FF?style=for-the-badge"/>
  <img src="https://img.shields.io/badge/Architecture-MVVM-orange?style=for-the-badge"/>
  <img src="https://img.shields.io/badge/License-Apache_2.0-blue?style=for-the-badge"/>
</p>

*Minimal • Native • Modern*

</div>

---

## Overview

Mercurio is a native Windows application built with **C#**, **.NET**, and **Windows Presentation Foundation (WPF)**.

The project aims to rethink how desktop software should feel—combining the reliability of native Windows applications with a modern, polished interface that prioritizes speed, clarity, and usability.

Instead of recreating traditional enterprise software, Mercurio focuses on creating an experience that feels lightweight, responsive, and enjoyable to use.

---

## Vision

Mercurio is built around a simple principle:

> **Powerful desktop software shouldn't feel complicated.**

The long-term goal is to provide a modern workspace where users can manage daily operations through a beautiful interface, fluid interactions, and an architecture prepared for future intelligent features.

---

## Design Principles

The interface follows a modern desktop philosophy inspired by contemporary product design.

- Clean and uncluttered layouts
- Fluent animations
- Consistent spacing
- Modern typography
- Rounded components
- Light and Dark themes
- Native Windows experience

---

## Features

### Current

- Modern authentication interface
- Animated Login ↔ Sign Up transition
- Native WPF application
- Theme-ready design
- Responsive desktop layouts

### Planned

- Dashboard
- User management
- Notifications
- Workspace modules
- Settings system
- AI-assisted workflows
- Analytics

---

## Tech Stack

| Technology | Purpose |
|------------|---------|
| **C#** | Programming Language |
| **.NET 9** | Runtime |
| **WPF** | Desktop UI |
| **XAML** | Interface Definition |
| **MVVM** | Architecture Pattern |

---

## Architecture

Mercurio follows the **MVVM (Model–View–ViewModel)** pattern.

```text
Views (XAML)
      │
      ▼
ViewModels
      │
      ▼
Services
      │
      ▼
Models
```

This separation keeps the UI independent from business logic, making the application easier to maintain, test, and expand.

---

## Project Structure

```text
Mercurio/
│
├── Assets/         # Icons, images and resources
├── Models/         # Data models
├── Services/       # Business logic
├── Themes/         # Colors and styles
├── ViewModels/     # MVVM ViewModels
├── Views/          # WPF Views
├── App.xaml
├── App.xaml.cs
└── README.md
```

---

## Getting Started

### Requirements

- Windows 10 or Windows 11
- .NET 9 SDK
- Visual Studio 2022

### Clone the repository

```bash
git clone https://github.com/brandonbert8/mercurio.git
cd mercurio
```

### Run

Open `Mercurio.sln` in Visual Studio and press **F5**.

Or run using the .NET CLI:

```bash
dotnet run
```

---

## Preview

> Screenshots will be added as development progresses.

| Login | Sign Up |
|------|---------|
| Coming Soon | Coming Soon |

---

## Roadmap

- [x] Project foundation
- [x] Design system
- [x] Authentication UI
- [ ] Dashboard
- [ ] Workspace modules
- [ ] Settings
- [ ] AI features
- [ ] Stable release

---

## Contributing

Contributions are welcome.

1. Fork the repository.
2. Create a feature branch.
3. Commit your changes.
4. Open a Pull Request.

Please keep changes consistent with the project's architecture and design principles.

---

## License

This project is licensed under the **Apache License 2.0**.

See the [LICENSE](LICENSE) file for more information.

---

<div align="center">

**Built by Brandon Bert**

*Crafting modern native software for Windows.*

</div>
