# Impinj Octane Implementation  

## 📌 Sobre o Projeto  
Este projeto é uma implementação do **Impinj Octane SDK** utilizando **C#**. Ele permite a leitura, escrita e manipulação de dados de tags RFID por meio de dispositivos compatíveis com a tecnologia Impinj.  

## 🚀 Funcionalidades  
- 📡 **Leitura de tags RFID**  
- ✍ **Escrita de dados nas tags**  
- 📊 **Processamento e filtragem de dados**  
- 🔄 **Regras para comunicação com o servidor**  

## 🛠️ Tecnologias Utilizadas  
- **C# (.NET Framework/.NET Core)**  
- **Impinj Octane SDK**  
- **PostgreSQL (caso tenha integração com banco de dados)**  

## 🏗️ Estrutura do Projeto  
```
📂 Impinj-Octane-Implementation
 ┣ 📂 gpo-find-tag          # Implementação da busca por tags
 ┣ 📂 read-process-data     # Processamento de dados lidos
 ┣ 📂 report-server-rule    # Regras de comunicação com o servidor
 ┣ 📂 tag-counter-prefix    # Contador de tags prefixadas
 ┣ 📂 write-tag-data        # Escrita de dados nas tags
 ┣ 📜 Impinj-Octane-Implementation.sln  # Solução do projeto
 ┣ 📜 utils.txt             # Arquivo auxiliar
 ┗ 📜 .gitignore            # Arquivo de configuração do Git
```

## ⚙️ Como Configurar  
1️⃣ **Clone o repositório**  
```bash
git clone https://github.com/LucasDiasJorge/Impinj-Octane-Implementation.git
```  

2️⃣ **Instale as dependências**  
- Certifique-se de ter o **.NET SDK** instalado  
- Instale o **Impinj Octane SDK** (se necessário)  

3️⃣ **Configure a conexão com o leitor RFID**  
- Verifique as configurações no código-fonte (IP do leitor, porta, etc.)  

4️⃣ **Compile e execute o projeto**  
```bash
dotnet build
dotnet run
```  
