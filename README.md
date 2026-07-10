# FIAP Cloud Games - PaymentsAPI

Microsservico responsavel por consumir pedidos de compra, simular o processamento do pagamento e publicar o resultado para os outros servicos da plataforma.

Este repositorio faz parte da Fase 2 do Tech Challenge e representa o microsservico independente de pagamentos.

## Responsabilidades

- Consumir `OrderPlacedEvent` publicado pela CatalogAPI.
- Simular pagamento de pedido com status `Approved` ou `Rejected`.
- Publicar `PaymentProcessedEvent`.
- Permitir que CatalogAPI confirme a biblioteca do usuario quando o pagamento for aprovado.
- Permitir que NotificationsAPI envie a confirmacao de compra quando o pagamento for aprovado.
- Expor endpoints de health para validacao local, Docker e Kubernetes.

## Tecnologias

- .NET 10
- ASP.NET Core Minimal API
- RabbitMQ
- MassTransit
- Swagger / OpenAPI
- Docker
- xUnit, NSubstitute e Shouldly

## Estrutura

```text
src/PaymentsAPI/
  Application/     Use cases da regra de pagamento.
  Consumers/       Consumers MassTransit dos eventos de integracao.
  Contracts/       Contratos de eventos compartilhados entre microsservicos.
  Health/          Checagens de prontidao do servico.
  Options/         Options tipadas para configuracoes externas.
  Program.cs       Configuracao da API, Swagger, MassTransit e health endpoints.

tests/PaymentsAPI.Tests/
  Application/     Testes dos use cases.
  Consumers/       Testes dos consumers.
  Health/          Testes dos health checkers.
  Options/         Testes das options.
```

## Variaveis de ambiente

| Variavel | Finalidade | Padrao local |
|---|---|---|
| `RabbitMq__Host` | Host do RabbitMQ usado pelo MassTransit. | `localhost` |
| `RabbitMq__Port` | Porta TCP usada pelo MassTransit e pelo health check do RabbitMQ. | `5672` |
| `RabbitMq__VirtualHost` | Virtual host do RabbitMQ. | `/` |
| `RabbitMq__Username` | Usuario do RabbitMQ. | `guest` |
| `RabbitMq__Password` | Senha do RabbitMQ. | `guest` |
| `PaymentSimulation__DefaultStatus` | Resultado simulado do pagamento. Valores aceitos: `Approved` ou `Rejected`. | `Approved` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execucao da API. | `Development` |

Configuracao local padrao em `src/PaymentsAPI/appsettings.json`:

```json
{
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "PaymentSimulation": {
    "DefaultStatus": "Approved"
  }
}
```

Observacao: os segredos deste repositorio sao apenas para desenvolvimento local academico.

## Executar a API localmente

Antes de executar a PaymentsAPI, mantenha um RabbitMQ rodando. Voce pode usar o RabbitMQ do repositorio de orquestracao ou de outro microsservico, desde que esteja acessivel em `localhost:5672`.

```powershell
cd C:\Projetos\FIAP\Projetos\fiap-cloud-games-payments-api

dotnet run --project src\PaymentsAPI\PaymentsAPI.csproj
```

URLs locais padrao:

```text
Swagger: http://localhost:5163/swagger
Health:  http://localhost:5163/health
Live:    http://localhost:5163/health/live
Ready:   http://localhost:5163/health/ready
```

O endpoint `/health/live` confirma que o processo da API esta de pe.
O endpoint `/health` e o `/health/ready` tentam abrir conexao TCP com o RabbitMQ configurado.

## Swagger

A PaymentsAPI nao possui endpoints de negocio neste momento, porque seu trabalho principal e consumir e publicar eventos via RabbitMQ. Mesmo assim, o Swagger esta habilitado para expor os endpoints operacionais, como health checks.

```text
http://localhost:5163/swagger
```

## Evento consumido

### `OrderPlacedEvent`

Publicado pela CatalogAPI quando o usuario inicia a compra de um ou mais jogos.

```json
{
  "orderId": "guid",
  "userId": "guid",
  "games": [
    {
      "gameId": "guid",
      "price": 49.90
    },
    {
      "gameId": "guid",
      "price": 24.90
    }
  ],
  "totalPrice": 74.80,
  "createdAt": "datetime"
}
```

## Evento publicado

### `PaymentProcessedEvent`

Publicado pela PaymentsAPI apos simular o processamento do pagamento.

```json
{
  "orderId": "guid",
  "userId": "guid",
  "games": [
    {
      "gameId": "guid",
      "price": 49.90
    },
    {
      "gameId": "guid",
      "price": 24.90
    }
  ],
  "totalPrice": 74.80,
  "status": "Approved",
  "processedAt": "datetime"
}
```

Quando `PaymentSimulation__DefaultStatus` esta como `Approved`, o fluxo feliz e acionado: CatalogAPI adiciona os jogos na biblioteca e NotificationsAPI simula e-mail de confirmacao.

## Fluxo de mensageria

```text
CatalogAPI
  publica OrderPlacedEvent
    ↓
PaymentsAPI
  consome OrderPlacedEvent
  executa ProcessOrderPaymentUseCase
  publica PaymentProcessedEvent
    ↓
CatalogAPI
  consome PaymentProcessedEvent e atualiza biblioteca

NotificationsAPI
  consome PaymentProcessedEvent e simula e-mail de compra aprovada
```

## Testes

Executar a suite:

```powershell
dotnet test PaymentsAPI.slnx -m:1
```

Os testes atuais cobrem:

- use case transformando `OrderPlacedEvent` em `PaymentProcessedEvent`;
- status simulado `Approved` e `Rejected`;
- consumer publicando `PaymentProcessedEvent`;
- defaults de `RabbitMqOptions`;
- defaults de `PaymentSimulationOptions`;
- checagem de conexao TCP usada pelo health readiness.

## Docker da API

Build da imagem:

```powershell
docker build -t fiap-cloud-games-payments-api:latest .
```

Executar a imagem apontando para um RabbitMQ rodando no host:

```powershell
docker run --rm -p 8084:8080 `
  -e ASPNETCORE_URLS=http://+:8080 `
  -e RabbitMq__Host=host.docker.internal `
  -e RabbitMq__Port=5672 `
  -e RabbitMq__VirtualHost=/ `
  -e RabbitMq__Username=guest `
  -e RabbitMq__Password=guest `
  -e PaymentSimulation__DefaultStatus=Approved `
  fiap-cloud-games-payments-api:latest
```

Acessar:

```text
http://localhost:8084/swagger
http://localhost:8084/health
```

## Kubernetes

Este microsservico deve ter manifests em `k8s/` com:

- `Deployment`
- `Service`
- `ConfigMap`
- `Secret`

No cluster, configure `RabbitMq__Host` com o nome do Service do RabbitMQ, por exemplo `rabbitmq`, e mantenha dados sensiveis em `Secret`.

Exemplo de comandos quando os manifests forem adicionados:

```powershell
kubectl apply -f .\k8s
kubectl get pods
kubectl get services
kubectl logs deployment/payments-api
```

## Problemas comuns

### `/health` retorna `Unhealthy`

Verifique se o RabbitMQ esta rodando e se a porta esta correta:

```powershell
docker ps
```

Tambem confira `RabbitMq__Host` e `RabbitMq__Port`.

### O pagamento sempre fica aprovado

Esse e o comportamento padrao para facilitar o teste do fluxo completo. Para simular rejeicao, configure:

```powershell
$env:PaymentSimulation__DefaultStatus="Rejected"
dotnet run --project src\PaymentsAPI\PaymentsAPI.csproj
```

### Nao aparece mensagem parada na fila

Isso e normal quando o consumer esta funcionando. O RabbitMQ entrega a mensagem para a PaymentsAPI e, depois do ACK, a mensagem sai da fila. Para acompanhar o fluxo, olhe os contadores da fila no RabbitMQ Management e os logs da API.
