[![MIT License][license-shield]][license-url]

<br />
<div align="center">
  <h1 align="center">nyxon</h1>

  <p align="center">
    A web-based, real-time communication platform built around zero-trust architecture and end-to-end encryption.
    <br />
  </p>
</div>

## About The Project

Modern commercial communication platforms often operate on a model of delegated trust, requiring users to rely on the promises and operational security of a service provider. This application is built to explore an alternative approach: minimizing the need for server-side trust as much as technically feasible.

### The Philosophy of Zero-Trust

This platform is engineered with the goal of shifting security away from server compliance and placing it directly into the hands of the clients. The backend infrastructure is designed to function primarily as a blind router of encrypted bytes. By design, the server does not possess the capability to read messages, it does not store master derivation keys, and it cannot easily forge user identities. The security model seeks to rely on verifiable cryptographic primitives rather than a service provider's internal policies.

### Cryptographic Foundation

To achieve End-to-End Encryption (E2EE) without requiring users to install native desktop applications, this platform implements complex cryptographic operations directly within the web browser. The system leverages established algorithms and protocols to secure the communication layer:

* **Key Agreement & Ratcheting:** The application utilizes mechanisms based on asynchronous cryptographic protocols—specifically leveraging X3DH and a ratcheting mechanism. These are implemented to provide strong security properties, including Forward Secrecy and, partially, Post-Compromise Security.
* **Asymmetric Cryptography:** Core identity and key exchange operations are built upon elliptic curve cryptography, primarily utilizing Curve25519.
* **Key Derivation:** Client-side key generation and the protection of local cryptographic material are secured using memory-hard derivation functions, specifically Argon2id.
* **Authenticated Encryption:** Data confidentiality and integrity for message payloads and local storage are handled via standard symmetric encryption primitives, such as AES-GCM.

By orchestrating these operations entirely within the client's local memory footprint, the platform aims to deliver a highly secure communication environment accessible through a standard web URL.

### Built With

* [![DotNet][DotNet-shield]][DotNet-url]
* [![AspNetCore][AspNetCore-shield]][AspNetCore-url]
* [![Blazor][Blazor-shield]][Blazor-url]
* [![PostgreSQL][Postgres-shield]][Postgres-url]
* [![Valkey][Valkey-shield]][Valkey-url]

## License

Distributed under the MIT License. See `LICENSE` for more information.

[license-shield]: https://img.shields.io/badge/License-MIT-black.svg?style=for-the-badge
[license-url]: https://opensource.org/licenses/MIT
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/your_username
[DotNet-shield]: https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white
[DotNet-url]: https://dotnet.microsoft.com/
[AspNetCore-shield]: https://img.shields.io/badge/ASP.NET_Core-512BD4?style=for-the-badge&logo=aspnet&logoColor=white
[AspNetCore-url]: https://dotnet.microsoft.com/en-us/apps/aspnet
[Blazor-shield]: https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor&logoColor=white
[Blazor-url]: https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor
[Postgres-shield]: https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white
[Postgres-url]: https://www.postgresql.org/
[Valkey-shield]: https://img.shields.io/badge/Valkey-DC382D?style=for-the-badge&logo=redis&logoColor=white
[Valkey-url]: https://valkey.io/
