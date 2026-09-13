# 🐾 CLYVO DAY API

API REST desenvolvida em **ASP.NET Core e C#** para o projeto **CLYVO DAY**, uma plataforma de acompanhamento contínuo da saúde e da rotina de pets.

O **CLYVO DAY** busca transformar o acompanhamento do pet em uma jornada contínua, aproximando tutores, veterinários e os registros importantes da vida do animal.

A API fornece a base para que informações de saúde, rotina, cuidados e acontecimentos importantes possam ser registradas e acompanhadas de forma estruturada ao longo do tempo.

O projeto foi desenvolvido com foco em boas práticas de desenvolvimento backend, autenticação, persistência de dados, observabilidade, monitoramento, logging estruturado e testes automatizados.

---

## 📌 Sobre o projeto

O **CLYVO DAY** é uma plataforma de continuidade do cuidado pet que permite registrar e acompanhar informações importantes ao longo da vida do animal.

A proposta vai além de armazenar dados cadastrais: a aplicação funciona como um diário digital de saúde e bem-estar, permitindo acompanhar eventos clínicos, rotina, comportamento, alimentação, peso, medicações e outros acontecimentos relevantes.

Existem dois principais tipos de usuário:

* **Tutor** — responsável pelo acompanhamento diário do pet.
* **Veterinário** — profissional responsável pelo acompanhamento clínico.

A plataforma também possui mecanismos de engajamento, como pontuação e conquistas, incentivando o tutor a manter registros frequentes sobre seus pets.

---

# 🚀 Tecnologias utilizadas

### Backend

* C#
* .NET / ASP.NET Core Web API
* Entity Framework Core
* Oracle Database
* Oracle Entity Framework Core Provider
* LINQ

### API e documentação

* REST
* OpenAPI
* Scalar

### Autenticação e segurança

* JWT Bearer Authentication
* ASP.NET Core Authentication
* Password Hashing
* Authorization

### Observabilidade

* Serilog
* OpenTelemetry
* ASP.NET Core Health Checks
* Structured Logging
* Correlation ID
* Distributed Tracing
* Métricas customizadas

### Testes

* xUnit
* Moq
* EF Core InMemory
* `WebApplicationFactory<Program>`
* Testes Unitários
* Testes de Integração
* Fixtures
* Collection Fixtures

---

# 🏗️ Arquitetura

A aplicação foi organizada separando responsabilidades entre as principais camadas.

```text
HTTP Request
     │
     ▼
┌─────────────────┐
│   Controllers   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Services     │
│ Regras de negócio
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  AppDbContext   │
│ Entity Framework│
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Oracle Database │
└─────────────────┘
```

### Controllers

Responsáveis por:

* receber requisições HTTP;
* validar informações relacionadas à requisição;
* identificar o usuário autenticado quando necessário;
* chamar a camada de Service;
* retornar os códigos HTTP apropriados.

### Services

Concentram as principais regras de negócio da aplicação, incluindo:

* validações;
* controle de acesso aos registros;
* pontuação de engajamento;
* gerenciamento de usuários;
* criação e consulta dos registros;
* logging estruturado;
* tracing;
* métricas.

### Domain

Contém as entidades e regras pertencentes ao domínio da aplicação.

### Data

Contém o `AppDbContext` e as configurações do Entity Framework Core responsáveis pelo mapeamento das entidades para o Oracle.

### Infrastructure

Responsável pelas configurações relacionadas à infraestrutura e observabilidade da aplicação, incluindo OpenTelemetry.

---

# 📂 Estrutura da solução

```text
ClyvoDayApiWeb.sln
│
├── ClyvoDayApiWeb
│   ├── Controllers
│   ├── Data
│   ├── Domain
│   │   ├── Enums
│   │   └── Models
│   ├── Services
│   ├── Program.cs
│   └── appsettings.json
│
├── ClyvoDayApiWeb.Infrastructure
│   └── Observability
│
├── ClyvoDayApiWeb.UnitTests
│   └── Services
│
└── ClyvoDayApiWeb.IntegrationTests
    ├── Controllers
    └── FactoryFixture
```

Os testes foram separados entre **Unit Tests** e **Integration Tests**, permitindo testar isoladamente as regras de negócio e também o comportamento completo da API através de requisições HTTP.

---

# 🧩 Entidades

## User

Classe base dos usuários do sistema.

Principais propriedades:

* `UserId`
* `FullName`
* `Email`
* `PasswordHash`
* `PhoneNumber`
* `TypeUser`
* `IsActive`
* `CreatedAt`
* `UpdatedAt`

`User` é utilizada como classe base para:

```text
User
├── Tutor
└── Veterinarian
```

A herança é persistida utilizando tabelas específicas para os tipos de usuário.

---

## Tutor

Representa o responsável pelo pet.

Além das informações herdadas de `User`, possui:

* `ScoreEngagement`
* `Achievement`
* coleção de `Pets`

O tutor recebe pontos ao utilizar determinadas funcionalidades da plataforma.

---

## Veterinarian

Representa um médico veterinário cadastrado.

Além das informações de `User`, possui:

* `Crmv`
* `State`
* `Specialty`

O CRMV é validado considerando a combinação entre número e estado.

---

## Pet

Representa um pet cadastrado por um tutor.

Principais propriedades:

* `PetId`
* `TutorId`
* `Name`
* `Species`
* `Breed`
* `Sex`
* `Age`
* `BirthDate`

Um pet pode possuir diversos registros relacionados à sua rotina e saúde.

---

## PetMonitoring

Representa um monitoramento do estado atual do pet.

Pode armazenar informações como:

* humor;
* nível de energia;
* hidratação;
* alimentação;
* qualidade do sono;
* atividades recentes;
* sociabilidade;
* uso de medicamento;
* peso;
* observações.

---

## DailyPetLog

Representa um registro do diário do pet.

Principais informações:

* pet relacionado;
* usuário responsável pelo registro;
* tipo do registro;
* conteúdo;
* imagem;
* privacidade;
* data do registro.

---

## CareEvent

Representa um evento de cuidado relacionado ao pet.

Exemplos:

* vacinação;
* consulta;
* exame;
* medicação;
* procedimentos.

Um evento possui status e pode ser:

```text
Scheduled
    │
    ├──► Completed
    │
    └──► Cancelled
```

Eventos concluídos não podem ser cancelados e eventos cancelados não podem ser concluídos.

---

## CommunityPost

Representa uma publicação realizada na comunidade.

Pode conter:

* categoria;
* conteúdo;
* imagem;
* localização;
* autor;
* data de publicação.

---

# 🔗 Relacionamentos

Os principais relacionamentos são:

```text
User
├── Tutor
│    │
│    └── 1:N Pet
│             │
│             ├── 1:N PetMonitoring
│             ├── 1:N CareEvent
│             └── 1:N DailyPetLog
│
└── Veterinarian

User
└── 1:N CommunityPost
```

Em resumo:

| Entidade             | Relacionamento |
| -------------------- | -------------- |
| Tutor → Pet          | 1:N            |
| Pet → PetMonitoring  | 1:N            |
| Pet → CareEvent      | 1:N            |
| Pet → DailyPetLog    | 1:N            |
| User → CommunityPost | 1:N            |

As relações principais possuem configuração de exclusão em cascata quando apropriado.

---

# 🎮 Sistema de engajamento

O CLYVO DAY possui um sistema de pontuação para incentivar o tutor a registrar informações frequentemente.

Algumas ações geram pontos:

| Ação                 | Pontos |
| -------------------- | -----: |
| Criar Daily Pet Log  |    +10 |
| Criar Pet Monitoring |    +15 |
| Criar Community Post |    +10 |

Conforme a pontuação aumenta, novas conquistas são liberadas:

| Pontuação | Conquista           |
| --------: | ------------------- |
|      0–49 | Nenhum              |
|       50+ | Iniciante Atencioso |
|      100+ | Tutor Dedicado      |
|      150+ | Guardião Pet        |
|      300+ | Clyvo Master        |

---

# 🔐 Autenticação

A API utiliza autenticação baseada em **JWT (JSON Web Token)**.

O fluxo de autenticação é:

```text
Login
  │
  ▼
POST /api/Auth/login
  │
  ▼
Validação das credenciais
  │
  ▼
Geração do JWT
  │
  ▼
Token + dados do usuário
  │
  ▼
Authorization: Bearer TOKEN
```

As senhas não são armazenadas em texto puro. Antes da persistência, são processadas utilizando password hashing.

O JWT contém informações utilizadas para identificar o usuário nas requisições protegidas.

---

# 🌐 Endpoints

## Auth

| Método | Endpoint          | Descrição                     |
| ------ | ----------------- | ----------------------------- |
| POST   | `/api/Auth/login` | Realiza login                 |
| GET    | `/api/Auth/me`    | Retorna o usuário autenticado |

## User

| Método | Endpoint                    | Descrição                |
| ------ | --------------------------- | ------------------------ |
| GET    | `/api/User`                 | Lista usuários           |
| GET    | `/api/User?type={type}`     | Filtra usuários por tipo |
| GET    | `/api/User/{id}`            | Busca usuário por ID     |
| PUT    | `/api/User/{id}/email`      | Atualiza e-mail          |
| PUT    | `/api/User/{id}/phone`      | Atualiza telefone        |
| PUT    | `/api/User/{id}/deactivate` | Desativa usuário         |
| DELETE | `/api/User/{id}`            | Exclui usuário           |

## Tutor

| Método | Endpoint          | Descrição      |
| ------ | ----------------- | -------------- |
| GET    | `/api/Tutor`      | Lista tutores  |
| GET    | `/api/Tutor/{id}` | Busca tutor    |
| POST   | `/api/Tutor`      | Cadastra tutor |

## Veterinarian

| Método | Endpoint                 | Descrição            |
| ------ | ------------------------ | -------------------- |
| GET    | `/api/Veterinarian`      | Lista veterinários   |
| GET    | `/api/Veterinarian/{id}` | Busca veterinário    |
| POST   | `/api/Veterinarian`      | Cadastra veterinário |

## Pet

| Método | Endpoint                   | Descrição                   |
| ------ | -------------------------- | --------------------------- |
| GET    | `/api/Pet`                 | Lista pets                  |
| GET    | `/api/Pet/{id}`            | Busca pet                   |
| GET    | `/api/Pet/tutor/{tutorId}` | Lista pets de um tutor      |
| GET    | `/api/Pet/my`              | Lista pets do usuário atual |
| POST   | `/api/Pet`                 | Cadastra pet                |

## Pet Monitoring

| Método | Endpoint                         | Descrição                   |
| ------ | -------------------------------- | --------------------------- |
| GET    | `/api/PetMonitoring`             | Lista monitoramentos        |
| GET    | `/api/PetMonitoring/{id}`        | Busca monitoramento         |
| GET    | `/api/PetMonitoring/pet/{petId}` | Lista monitoramentos do pet |
| POST   | `/api/PetMonitoring`             | Cria monitoramento          |
| DELETE | `/api/PetMonitoring/{id}`        | Remove monitoramento        |

## Daily Pet Log

| Método | Endpoint                       | Descrição              |
| ------ | ------------------------------ | ---------------------- |
| GET    | `/api/DailyPetLog`             | Lista registros        |
| GET    | `/api/DailyPetLog/{id}`        | Busca registro         |
| GET    | `/api/DailyPetLog/pet/{petId}` | Lista registros do pet |
| POST   | `/api/DailyPetLog`             | Cria registro          |
| DELETE | `/api/DailyPetLog/{id}`        | Remove registro        |

## Care Event

| Método | Endpoint                       | Descrição            |
| ------ | ------------------------------ | -------------------- |
| GET    | `/api/CareEvent`               | Lista eventos        |
| GET    | `/api/CareEvent/{id}`          | Busca evento         |
| GET    | `/api/CareEvent/pet/{petId}`   | Lista eventos do pet |
| POST   | `/api/CareEvent`               | Cria evento          |
| PUT    | `/api/CareEvent/{id}/complete` | Conclui evento       |
| PUT    | `/api/CareEvent/{id}/cancel`   | Cancela evento       |
| DELETE | `/api/CareEvent/{id}`          | Remove evento        |

## Community Post

| Método | Endpoint                           | Descrição                       |
| ------ | ---------------------------------- | ------------------------------- |
| GET    | `/api/CommunityPost`               | Lista publicações               |
| GET    | `/api/CommunityPost/{id}`          | Busca publicação                |
| GET    | `/api/CommunityPost/user/{userId}` | Lista publicações de um usuário |
| POST   | `/api/CommunityPost`               | Cria publicação                 |
| DELETE | `/api/CommunityPost/{id}`          | Remove publicação               |

---

# 🩺 Health Checks

A aplicação possui endpoints específicos para monitoramento de saúde.

## Liveness

```http
GET /health/live
```

Verifica se a aplicação está em execução.

Este endpoint é útil para determinar se o processo da API continua ativo.

---

## Readiness

```http
GET /health/ready
```

Verifica se a aplicação está pronta para receber requisições e considera dependências configuradas para readiness, incluindo a conectividade com o banco de dados.

---

## Detalhes

```http
GET /health/details
```

Fornece informações mais detalhadas sobre o estado dos Health Checks configurados.

Entre os checks utilizados estão:

```text
API Running
Oracle Database
```

Os Health Checks utilizam:

```text
Microsoft.Extensions.Diagnostics.HealthChecks
```

Exemplo de configuração:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "Oracle Database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "database", "oracle", "ready" })
    .AddCheck(
        "API Running",
        () => HealthCheckResult.Healthy(),
        tags: new[] { "live" });
```

---

# 📊 Observabilidade e monitoramento

A aplicação possui três pilares principais de observabilidade:

```text
Logs
Tracing
Metrics
```

---

## 📝 Logging estruturado

O projeto utiliza **Serilog** para logging estruturado.

São utilizados diferentes níveis:

```text
Information
Warning
Error
```

Exemplo:

```csharp
_logger.LogInformation(
    "Usuário {UserId} desativado com sucesso.",
    userId);
```

A utilização de propriedades estruturadas como `{UserId}` permite pesquisar e analisar logs com maior facilidade.

Os logs são enviados para:

* console;
* arquivos de log.

A aplicação também utiliza:

```csharp
app.UseSerilogRequestLogging();
```

para registrar automaticamente informações sobre as requisições HTTP.

Por isso, não é necessário adicionar logs repetitivos em cada Controller. Eventos e regras de negócio são registrados principalmente nos Services, enquanto as requisições HTTP são registradas globalmente pelo middleware.

---

# 🔗 Correlation ID

Cada requisição pode possuir um identificador de correlação.

Header utilizado:

```http
X-Correlation-ID
```

Esse identificador permite acompanhar uma mesma requisição através dos diferentes logs gerados durante seu processamento.

Exemplo:

```text
Request
   │
   ├── X-Correlation-ID: abc-123
   │
   ▼
Controller
   │
   ▼
Service
   │
   ▼
Database
```

Isso facilita a investigação de erros e o rastreamento de requisições.

---

# 🔭 OpenTelemetry

O projeto utiliza **OpenTelemetry** para tracing e métricas.

A instrumentação inclui:

* ASP.NET Core;
* HttpClient;
* Activities customizadas;
* métricas customizadas;
* exportação para console.

As Activities permitem acompanhar operações importantes executadas nas diferentes camadas da aplicação.

---

# 📈 Métricas

Além das métricas fornecidas pela instrumentação do ASP.NET Core, foram adicionados contadores relacionados às principais operações da aplicação.

Exemplos:

```text
pets_created_total
pet_monitorings_created_total
daily_pet_logs_created_total
care_events_created_total
community_posts_created_total
```

Essas métricas permitem acompanhar a utilização das principais funcionalidades.

A instrumentação do ASP.NET Core também permite observar informações relacionadas ao processamento das requisições, como duração e comportamento das requisições HTTP.

---

# 🖥️ Como monitorar a aplicação

Durante a execução da API, é possível utilizar diferentes mecanismos.

### 1. Health Checks

Acessar:

```text
/health/live
/health/ready
/health/details
```

### 2. Console

O console apresenta:

* logs do Serilog;
* requisições HTTP;
* traces;
* informações exportadas pelo OpenTelemetry.

### 3. Arquivos de log

Os arquivos configurados pelo Serilog podem ser utilizados para analisar eventos históricos da aplicação.

### 4. Correlation ID

Ao investigar uma requisição específica, procure pelo mesmo `X-Correlation-ID` nos logs.

### 5. Métricas

Observe as métricas do OpenTelemetry para analisar requisições e operações executadas pela API.

---

# ⚙️ Como executar o projeto

## Pré-requisitos

Certifique-se de possuir:

* .NET SDK compatível com o projeto;
* Oracle Database disponível;
* acesso à instância Oracle configurada;
* Git, caso o projeto seja clonado do repositório.

Verifique o .NET:

```bash
dotnet --version
```

---

## 1. Clonar o repositório

```bash
git clone <URL_DO_REPOSITORIO>
cd <PASTA_DO_REPOSITORIO>
```

---

## 2. Restaurar dependências

Na raiz da solução:

```bash
dotnet restore
```

---

## 3. Configurar o banco de dados

Configure a connection string:

```text
OracleConnection
```

A configuração pode ser fornecida através do `appsettings.json` ou por variável de ambiente, conforme o ambiente utilizado.

Exemplo conceitual:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "<SUA_CONNECTION_STRING>"
  }
}
```

> ⚠️ Não publique credenciais reais, senhas do banco ou chaves JWT no GitHub.

---

## 4. Configurar JWT

A aplicação precisa de uma chave secreta para assinatura dos tokens JWT.

Mantenha secrets fora do código-fonte e do repositório público.

Em desenvolvimento, utilize configuração local ou variável de ambiente conforme a configuração utilizada pelo projeto.

---

## 5. Aplicar as migrations

```bash
dotnet ef database update --project ClyvoDayApiWeb
```

Caso seja necessário criar uma nova migration:

```bash
dotnet ef migrations add NomeDaMigration --project ClyvoDayApiWeb
```

---

## 6. Executar a API

```bash
dotnet run --project ClyvoDayApiWeb
```

No ambiente local configurado durante o desenvolvimento, a API utiliza a porta `10000`.

---

# 📖 Scalar

Com a aplicação executando localmente, a documentação interativa pode ser acessada em:

```text
http://localhost:10000/scalar
```

O Scalar permite visualizar e testar os endpoints da API.

---

# 🧪 Exemplos para testar no Scalar

## Cadastrar Tutor

```http
POST /api/Tutor
```

Body:

```json
{
  "fullName": "Maria Silva",
  "email": "maria@email.com",
  "password": "123456",
  "phoneNumber": "11999999999"
}
```

---

## Cadastrar Veterinário

```http
POST /api/Veterinarian
```

```json
{
  "fullName": "Carlos Souza",
  "email": "carlos@email.com",
  "password": "123456",
  "phoneNumber": "11988888888",
  "crmv": "12345",
  "state": "SP",
  "specialty": "Clínica Geral"
}
```

---

## Login

```http
POST /api/Auth/login
```

```json
{
  "email": "maria@email.com",
  "password": "123456"
}
```

Uma autenticação bem-sucedida retorna dados do usuário e um JWT.

Exemplo conceitual:

```json
{
  "message": "Login realizado com sucesso.",
  "userId": 1,
  "fullName": "Maria Silva",
  "email": "maria@email.com",
  "typeUser": "Tutor",
  "token": "JWT_TOKEN"
}
```

Para endpoints protegidos, envie:

```http
Authorization: Bearer JWT_TOKEN
```

---

## Cadastrar Pet

```http
POST /api/Pet
```

Exemplo:

```json
{
  "tutorId": 1,
  "name": "Luna",
  "species": "Gato",
  "breed": "SRD",
  "sex": "Fêmea",
  "age": 3,
  "birthDate": "2023-05-10"
}
```

---

## Criar monitoramento

```http
POST /api/PetMonitoring
```

```json
{
  "petId": 1,
  "mood": "Feliz",
  "energyLevel": "Alta",
  "hydrationLevel": "Normal",
  "food": "Comeu normalmente",
  "sleepQuality": "Boa",
  "recentActivities": "Brincou durante a manhã",
  "sociability": "Sociável",
  "tookMedication": false,
  "weight": 4.5,
  "observations": "Sem alterações"
}
```

---

## Criar registro diário

```http
POST /api/DailyPetLog
```

Exemplo:

```json
{
  "petId": 1,
  "dailyPetLogType": "Rotina",
  "content": "Luna brincou bastante durante a manhã.",
  "imageUrl": null,
  "privacy": 0
}
```

> O valor de `privacy` deve seguir os valores definidos no enum `EnumPrivacy` da aplicação.

---

## Criar evento de cuidado

```http
POST /api/CareEvent
```

```json
{
  "petId": 1,
  "typeEvent": "Vacinação",
  "description": "Vacina B12",
  "eventDate": "2026-10-10",
  "observations": "Nenhuma"
}
```

Para concluir:

```http
PUT /api/CareEvent/{id}/complete
```

Para cancelar:

```http
PUT /api/CareEvent/{id}/cancel
```

---

## Criar publicação na comunidade

```http
POST /api/CommunityPost
```

```json
{
  "category": "Dicas",
  "content": "Hoje tivemos um ótimo passeio!",
  "imageUrl": null,
  "location": "São Paulo"
}
```

---

# 🧪 Testes automatizados

A solução possui dois projetos dedicados aos testes.

```text
ClyvoDayApiWeb.UnitTests
ClyvoDayApiWeb.IntegrationTests
```

---

## Unit Tests

Os testes unitários utilizam:

* xUnit;
* Moq;
* EF Core InMemory;
* padrão AAA.

Estrutura utilizada:

```text
Arrange
Act
Assert
```

A nomenclatura segue o padrão:

```text
MetodoTestado_Cenario_ResultadoEsperado
```

Exemplo:

```text
UpdateEmailAsync_EmailDuplicado_DeveLancarArgumentException
```

Os testes unitários verificam regras como:

* validações;
* criação de entidades;
* usuários inexistentes;
* usuários inativos;
* e-mail duplicado;
* CRMV duplicado;
* autorização sobre registros;
* pontuação de engajamento;
* conquistas;
* transições de estado de eventos.

---

## Integration Tests

Os testes de integração utilizam:

```text
WebApplicationFactory<Program>
```

Eles executam requisições HTTP contra uma instância da aplicação criada especificamente para testes.

O banco Oracle é substituído por **EF Core InMemory** no ambiente de integração.

Também é utilizado um Authentication Handler específico para permitir testar endpoints autenticados sem depender da emissão de um JWT em todos os cenários.

O login real também possui testes próprios.

---

# ▶️ Executando os testes

Na raiz da solução, para executar todos os testes:

```bash
dotnet test
```

Para executar apenas os Unit Tests:

```bash
dotnet test ClyvoDayApiWeb.UnitTests
```

Para executar apenas os Integration Tests:

```bash
dotnet test ClyvoDayApiWeb.IntegrationTests
```

Uma execução bem-sucedida deve finalizar sem testes com status `Failed`.

---

# 🧪 Organização dos testes

```text
ClyvoDayApiWeb.UnitTests
└── Services
    ├── UserServiceTests
    ├── TutorServiceTests
    ├── VeterinarianServiceTests
    ├── PetServiceTests
    ├── PetMonitoringServiceTests
    ├── DailyPetLogServiceTests
    ├── CareEventServiceTests
    └── CommunityPostServiceTests

ClyvoDayApiWeb.IntegrationTests
├── FactoryFixture
│   └── ApiFactoryFixture.cs
│
└── Controllers
    ├── AuthControllerTests
    ├── UserControllerTests
    ├── TutorControllerTests
    ├── VeterinarianControllerTests
    ├── PetControllerTests
    ├── PetMonitoringControllerTests
    ├── DailyPetLogControllerTests
    ├── CareEventControllerTests
    └── CommunityPostControllerTests
```

Os testes de integração utilizam **Fixtures e Collection Fixtures** para compartilhar a infraestrutura necessária entre as classes de teste.

Exemplo:

```csharp
[CollectionDefinition("ApiCollection")]
public class ApiCollection : ICollectionFixture<ApiFactoryFixture>
{
}
```

---

# 🗄️ Banco de dados

O projeto utiliza **Oracle Database** através do Entity Framework Core.

O acesso ao banco é centralizado pelo:

```text
AppDbContext
```

A aplicação utiliza migrations para versionar alterações na estrutura do banco.

Principais tabelas:

```text
USERS
TUTORS
VETERINARIANS
PETS
PET_MONITORINGS
CARE_EVENTS
DAILY_PET_LOGS
COMMUNITY_POSTS
```

---

# 🛡️ Segurança

Algumas práticas utilizadas no projeto:

* password hashing;
* JWT;
* autenticação Bearer;
* autorização em endpoints protegidos;
* validações na camada de Service;
* não exposição intencional de senha em respostas;
* controle de acesso aos registros do pet;
* secrets externos ao código-fonte.

> Nunca faça commit de connection strings com credenciais reais, chaves JWT ou outros secrets.

---

# 📋 Códigos HTTP utilizados

A API utiliza códigos HTTP de acordo com o resultado da operação.

|                      Código | Significado                                      |
| --------------------------: | ------------------------------------------------ |
|                    `200 OK` | Operação realizada com sucesso                   |
|               `201 Created` | Recurso criado                                   |
|            `204 No Content` | Exclusão realizada sem conteúdo de resposta      |
|           `400 Bad Request` | Dados ou operação inválidos                      |
|          `401 Unauthorized` | Usuário não autenticado ou credenciais inválidas |
|             `403 Forbidden` | Usuário autenticado sem permissão                |
|             `404 Not Found` | Recurso não encontrado                           |
|              `409 Conflict` | Conflito ao executar determinada operação        |
| `500 Internal Server Error` | Erro inesperado                                  |

---

# 🌱 Variáveis e configurações importantes

Antes de executar a aplicação, verifique:

```text
OracleConnection
JWT Secret/Key
PORT
```

A porta pode ser definida através da variável de ambiente `PORT`. Quando não definida, o ambiente local do projeto utiliza a porta `10000`.

---

# 📚 Documentação da API

A documentação OpenAPI é apresentada através do **Scalar**.

Com a API executando:

```text
http://localhost:10000/scalar
```

A interface permite:

* consultar os endpoints;
* visualizar parâmetros;
* visualizar schemas;
* enviar requisições;
* testar autenticação;
* consultar respostas HTTP.

---

# 🎯 Objetivos técnicos

Além das funcionalidades do domínio, o projeto foi desenvolvido para demonstrar:

* construção de API REST com ASP.NET Core;
* integração com Oracle;
* Entity Framework Core;
* autenticação JWT;
* password hashing;
* arquitetura em camadas;
* regras de negócio;
* Health Checks;
* logging estruturado;
* correlação de requisições;
* distributed tracing;
* métricas;
* OpenTelemetry;
* testes unitários;
* mocking;
* testes de integração;
* `WebApplicationFactory`;
* Fixtures e Collection Fixtures.

---

