# CommerceEngine

A comprehensive product management system built with .NET microservices architecture, featuring product catalog, inventory control, and order processing with API Gateway, Docker containerization, and CI/CD pipeline.

## 🏗️ Arquitetura

Este projeto implementa uma arquitetura de microsserviços com os seguintes componentes:

### Microsserviços
- **Catalog.API** - Gerenciamento de catálogo de produtos ✅
- **Inventory.API** - Controle de estoque ✅
- **Order.API** - Processamento de pedidos ✅
- **Identity.API** - Autenticação e autorização ✅

### Infraestrutura
- **API Gateway** - Ponto de entrada único com Ocelot ✅
- **SQL Server** - Banco de dados relacional
- **Docker** - Containerização
- **Azure DevOps** - CI/CD (planejado)

## 🚀 Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - APIs RESTful
- **Entity Framework Core** - ORM
- **SQL Server** - Banco de dados
- **AutoMapper** - Mapeamento de objetos
- **Swagger/OpenAPI** - Documentação da API
- **Docker** - Containerização
- **Docker Compose** - Orquestração local

## 📋 Pré-requisitos

- .NET 8 SDK
- Docker Desktop
- SQL Server (LocalDB ou Docker)
- Visual Studio 2022 ou VS Code

## 🛠️ Como Executar

### Opção 1: Docker Compose (Recomendado)
```bash
# Clonar o repositório
git clone <repository-url>
cd CommerceEngine

# Executar com Docker Compose
docker-compose up -d

# As APIs estarão disponíveis em:
# - API Gateway: http://localhost:5000
# - Catalog API: http://localhost:5001
# - Inventory API: http://localhost:5002
# - Order API: http://localhost:5003
# - Identity API: http://localhost:5004
# - Swagger UIs: http://localhost:5000/swagger, http://localhost:5001, http://localhost:5002, http://localhost:5003 e http://localhost:5004
```

### Opção 2: Execução Local
```bash
# Catalog API
cd src/Services/Catalog.API
dotnet run
# Disponível em: https://localhost:7001

# Inventory API (em outro terminal)
cd src/Services/Inventory.API
dotnet run
# Disponível em: https://localhost:7002

# Order API (em outro terminal)
cd src/Services/Order.API
dotnet run
# Disponível em: https://localhost:7003
```

## 📚 Endpoints da API

### API Gateway (Porta 5000)

O API Gateway usando Ocelot atua como ponto de entrada único para todos os microsserviços:

| Rota Original | Rota no Gateway | Descrição |
|---------------|-----------------|-----------|
| `http://localhost:5001/api/products` | `http://localhost:5000/api/catalog/products` | Catalog API |
| `http://localhost:5002/api/inventory` | `http://localhost:5000/api/inventory` | Inventory API |
| `http://localhost:5003/api/orders` | `http://localhost:5000/api/orders` | Order API |
| `http://localhost:5004/api/auth` | `http://localhost:5000/api/auth` | Identity API |
| `http://localhost:5004/api/users` | `http://localhost:5000/api/users` | User Management |

**Funcionalidades do Gateway:**
- ✅ Roteamento inteligente para microsserviços
- ✅ Autenticação JWT centralizada
- ✅ Rate limiting configurável
- ✅ Logging e monitoramento
- ✅ CORS configurado
- ✅ Swagger UI integrado
- ✅ Load balancing (preparado)

### Catalog API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/products` | Lista todos os produtos |
| GET | `/api/products/{id}` | Obtém produto por ID |
| GET | `/api/products/sku/{sku}` | Obtém produto por SKU |
| GET | `/api/products/category/{category}` | Lista produtos por categoria |
| GET | `/api/products/categories` | Lista todas as categorias |
| POST | `/api/products` | Cria novo produto |
| PUT | `/api/products/{id}` | Atualiza produto |
| DELETE | `/api/products/{id}` | Remove produto |

### Inventory API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/inventory` | Lista todos os itens de inventário |
| GET | `/api/inventory/{id}` | Obtém item por ID |
| GET | `/api/inventory/product/{productId}` | Obtém item por Product ID |
| GET | `/api/inventory/sku/{sku}` | Obtém item por SKU |
| POST | `/api/inventory` | Cria novo item de inventário |
| PUT | `/api/inventory/{id}` | Atualiza item de inventário |
| DELETE | `/api/inventory/{id}` | Remove item de inventário |
| POST | `/api/inventory/stock/add` | Adiciona estoque |
| POST | `/api/inventory/stock/remove` | Remove estoque |
| POST | `/api/inventory/stock/reserve` | Reserva estoque |
| POST | `/api/inventory/stock/release` | Libera reserva |
| GET | `/api/inventory/stock/check/{productId}` | Verifica estoque |
| GET | `/api/inventory/stock/check/sku/{sku}` | Verifica estoque por SKU |
| GET | `/api/inventory/low-stock` | Lista itens com baixo estoque |
| GET | `/api/inventory/out-of-stock` | Lista itens fora de estoque |
| GET | `/api/inventory/location/{location}` | Lista por localização |

### Order API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/orders` | Lista todos os pedidos (resumo) |
| GET | `/api/orders/{id}` | Obtém pedido por ID (detalhado) |
| GET | `/api/orders/number/{orderNumber}` | Obtém pedido por número |
| GET | `/api/orders/customer/{customerId}` | Lista pedidos por cliente |
| POST | `/api/orders` | Cria novo pedido |
| PUT | `/api/orders/{id}` | Atualiza pedido |
| DELETE | `/api/orders/{id}` | Remove pedido (apenas pendentes) |
| PUT | `/api/orders/{id}/status` | Atualiza status do pedido |
| POST | `/api/orders/{id}/cancel` | Cancela pedido |
| POST | `/api/orders/{id}/complete` | Completa pedido |
| GET | `/api/orders/status/{status}` | Lista pedidos por status |
| GET | `/api/orders/pending` | Lista pedidos pendentes |
| GET | `/api/orders/processing` | Lista pedidos em processamento |
| GET | `/api/orders/recent` | Lista pedidos recentes |
| GET | `/api/orders/date-range` | Lista pedidos por período |
| GET | `/api/orders/statistics/sales` | Obtém estatísticas de vendas |
| GET | `/api/orders/{id}/can-cancel` | Verifica se pode cancelar |
| GET | `/api/orders/{id}/exists` | Verifica se pedido existe |

## 🗂️ Estrutura do Projeto

```
CommerceEngine/
├── src/
│   ├── Services/
│   │   ├── Catalog.API/
│   │   │   ├── Controllers/
│   │   │   ├── Models/
│   │   │   ├── DTOs/
│   │   │   ├── Services/
│   │   │   ├── Data/
│   │   │   ├── Mappings/
│   │   │   └── Dockerfile
│   │   ├── Inventory.API/
│   │   │   ├── Controllers/
│   │   │   ├── Models/
│   │   │   ├── DTOs/
│   │   │   ├── Services/
│   │   │   ├── Data/
│   │   │   ├── Mappings/
│   │   │   └── Dockerfile
│   │   ├── Order.API/
│   │   │   ├── Controllers/
│   │   │   ├── Models/
│   │   │   ├── DTOs/
│   │   │   ├── Services/
│   │   │   ├── Data/
│   │   │   ├── Mappings/
│   │   │   └── Dockerfile
│   │   └── Identity.API/
│   │       ├── Controllers/
│   │       ├── Models/
│   │       ├── DTOs/
│   │       ├── Services/
│   │       ├── Data/
│   │       ├── Mappings/
│   │       └── Dockerfile
│   └── ApiGateway/
│       ├── Controllers/
│       ├── ocelot.json
│       ├── ocelot.Development.json
│       ├── Program.cs
│       └── Dockerfile
├── docker/
├── k8s/
├── tests/
├── docker-compose.yml
└── README.md
```

## 🧪 Testes

```bash
# Executar testes unitários
dotnet test

# Executar testes de integração
dotnet test --filter Category=Integration
```

## 🔄 Próximos Passos

1. ✅ Implementar Catalog.API
2. ✅ Implementar Inventory.API
3. ✅ Implementar Order.API
4. ✅ Implementar Identity.API
5. ✅ Implementar API Gateway
6. 🔄 Configurar CI/CD no Azure DevOps
7. 🔄 Implementar testes automatizados
8. 🔄 Deploy no Azure
9. 🔄 Implementar Service Discovery
10. 🔄 Adicionar Circuit Breaker
11. 🔄 Implementar Distributed Tracing

## 🤝 Contribuição

Este é um projeto de estudos. Sinta-se à vontade para fazer fork e experimentar!

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.
