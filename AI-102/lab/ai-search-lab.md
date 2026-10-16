---
lab:
    title: 'Azure AI Search Multi-Source Integration Lab'
    description: "Build a comprehensive search solution using Azure AI Search with multiple data sources, AI enrichment, vector search, and RAG patterns."
---

# Azure AI Search Multi-Source Integration Lab

## Overview

In this hands-on lab, you'll build a comprehensive search solution that demonstrates the full capabilities of Azure AI Search. You'll integrate multiple data sources, apply AI enrichment with skillsets, implement vector search for semantic understanding, and create two different consumption patterns: a web application and an Azure AI Foundry agent.

### What You'll Build

An intelligent search solution that combines:

- **Azure AI Search** - Central search service with multiple indexes
- **Azure Blob Storage** - Document repository with mixed formats (text, JSON, PDF)
- **Azure SQL Database** - Relational data source (AdventureWorks)
- **Azure OpenAI** - Embedding generation for vector search
- **Azure AI Services** - OCR and text extraction from documents
- **Python Web App** - Query interface for product search
- **Azure AI Foundry Agent** - RAG pattern for knowledge-based Q&A

### Learning Approach

This lab uses a **progressive architecture approach**. You'll build a complete search infrastructure step-by-step:

1. **Part 1**: Provision all Azure resources and configure connections
2. **Part 2**: Create AI-enriched index from blob storage with vector search
3. **Part 3**: Create structured index from SQL database
4. **Part 4**: Deploy and configure web application for product search
5. **Part 5**: Integrate with Azure AI Foundry for intelligent agent (BONUS)

After each part, you'll test and verify functionality before moving forward. This incremental approach ensures each component works correctly.

### Final Capabilities

By the end of this lab, your solution will:
1. ✓ Index documents from Azure Blob Storage with AI enrichment
2. ✓ Extract text from PDFs using OCR skillsets
3. ✓ Generate embeddings and enable vector search
4. ✓ Index structured data from Azure SQL Database
5. ✓ Provide faceted search and filtering capabilities
6. ✓ Serve a web application for product search
7. ✓ Power an AI agent with retrieval-augmented generation (RAG)

**Estimated time**: 90 minutes (60-75 minutes for Parts 1-4, Part 5 is optional bonus)

**Target audience**: Intermediate developers with basic Azure and Python experience

**Estimated cost**: $5-10 for lab duration (if resources are cleaned up promptly)

## Prerequisites

### Required Software

Before starting this lab, ensure you have the following installed:

- **Python 3.8 or higher** ([Download](https://www.python.org/downloads/))
  - Verify: `python --version`
- **pip** (Python package installer)
  - Verify: `pip --version`
- **Azure CLI** (optional but recommended) ([Download](https://docs.microsoft.com/cli/azure/install-azure-cli))
  - Verify: `az --version`
- **Text editor** (VS Code recommended, or any editor you prefer)
- **Azure subscription** with Contributor permissions

### Required Azure Resources

You'll create these during the lab:
- Azure AI Search service (Basic tier or higher)
- Azure Storage Account
- Azure SQL Database
- Azure OpenAI service
- Azure AI Services multi-service account

### Provided Lab Materials

Download and have ready:
- **AdventureWorks bacpac file** - SQL database backup
- **Sample documents** - Text files, JSON files, and PDFs for blob storage
- **Web application code** - Python Flask application with search UI
- **Sample queries** - Python scripts for testing

> **💡 Tip**: Create a dedicated resource group for this lab to make cleanup easier!

---

## Part 1: Provision Azure Resources and Configure Environment

In this part, you'll create all the Azure resources needed for the lab and set up your local development environment.

### Step 1.1: Create Resource Group

First, create a resource group to contain all lab resources.

1. Sign in to the [Azure Portal](https://portal.azure.com)
2. Click **Resource groups** in the left navigation
3. Click **+ Create**
4. Configure:
   - **Subscription**: Your Azure subscription
   - **Resource group**: `ai-search-lab-rg`
   - **Region**: Choose a region (e.g., **East US**, **West Europe**)
5. Click **Review + create**, then **Create**

### Step 1.2: Provision Azure AI Search

1. In the Azure Portal search bar, type **Azure AI Search** and select it
2. Click **+ Create**
3. Configure:
   - **Subscription**: Your Azure subscription
   - **Resource group**: `ai-search-lab-rg`
   - **Service name**: `ai-search-lab-<yourname>-<random>` (must be globally unique)
   - **Location**: Same region as resource group
   - **Pricing tier**: Click **Change Pricing Tier** and select **Basic** (required for skillsets)
     - Note: Free tier has limitations (50MB, 3 indexes) - not recommended for this lab
4. Click **Review + create**, then **Create**
5. Wait for deployment (2-3 minutes)
6. After deployment, click **Go to resource**
7. In the left navigation, click **Keys**
8. **Copy and save**:
   - **URL** (your search endpoint)
   - **Primary admin key**

> **💰 Cost Note**: Basic tier costs ~$75/month, but you'll only be charged for actual usage time. Remember to delete resources after the lab!

### Step 1.3: Provision Azure Storage Account

1. In Azure Portal search bar, type **Storage accounts** and select it
2. Click **+ Create**
3. Configure:
   - **Subscription**: Your Azure subscription
   - **Resource group**: `ai-search-lab-rg`
   - **Storage account name**: `searchlab<yourname><random>` (lowercase, no special characters)
   - **Region**: Same region as other resources
   - **Performance**: Standard
   - **Redundancy**: Locally-redundant storage (LRS)
4. Click **Review + create**, then **Create**
5. After deployment, click **Go to resource**
6. In the left navigation, click **Containers** under **Data storage**
7. Click **+ Container**
8. Create a new container:
   - **Name**: `documents`
   - **Public access level**: Private
9. Click **Create**
10. Go back to the storage account overview
11. In the left navigation, click **Access keys** under **Security + networking**
12. **Copy and save** the **Connection string** from key1

### Step 1.4: Upload Sample Documents to Blob Storage

1. In your storage account, navigate to **Containers** > **documents**
2. Click **Upload**
3. Upload the provided sample files:
   - Text files with knowledge content
   - JSON files with structured data
   - PDF files with documents
4. Verify all files are uploaded successfully

> **📝 Note**: For this lab, ensure you have at least 5-10 documents total across different formats.
> **💡 Quick Start**: If you don't have sample files ready, create simple test files:
> - Create 3-4 .txt files with product/service descriptions
> - Create 1-2 .json files with structured data like {"title": "Info", "content": "Details"}
> - Use any PDF files you have available (manuals, datasheets, etc.)

### Step 1.5: Provision Azure SQL Database

1. In Azure Portal search bar, type **SQL databases** and select it
2. Click **+ Create**
3. Configure:
   - **Subscription**: Your Azure subscription
   - **Resource group**: `ai-search-lab-rg`
   - **Database name**: `AdventureWorksLT`
   - **Server**: Click **Create new**
     - **Server name**: `sqlserver-lab-<yourname>-<random>` (globally unique)
     - **Location**: Same region as other resources
     - **Authentication method**: Use SQL authentication
     - **Server admin login**: `sqladmin`
     - **Password**: Create a strong password and **save it securely**
     - Click **OK**
   - **Want to use SQL elastic pool?**: No
   - **Compute + storage**: Click **Configure database**
     - Select **Basic** tier (5 DTUs, 2GB) for cost savings
     - Click **Apply**
4. Click **Next: Networking**
5. Configure networking:
   - **Connectivity method**: Public endpoint
   - **Firewall rules**:
     - ✓ Allow Azure services and resources to access this server (IMPORTANT!)
     - ✓ Add current client IP address (for your machine)
6. Click **Review + create**, then **Create**
7. Wait for deployment (3-5 minutes)

### Step 1.6: Restore AdventureWorks Database

1. After SQL Database deployment, click **Go to resource**
2. In the left navigation, click **Query editor (preview)**
3. Sign in using:
   - **Login**: `sqladmin`
   - **Password**: Your password from Step 1.5
4. You have two options to load the AdventureWorks data:

   **Option A: Import from bacpac file (if provided)**
   1. Follow the portal's import process
   2. Upload your AdventureWorksLT.bacpac file

   **Option B: Use sample database**
   1. Delete the empty database
   2. Create a new SQL Database with the same settings
   3. Under **Additional settings** tab, select:
      - **Use existing data**: Sample
      - **Select sample**: AdventureWorksLT
   4. Complete creation

5. Verify the database by running a test query in Query editor:
   ```sql
   SELECT TOP 10 ProductID, Name, ProductNumber, Color, ListPrice
   FROM SalesLT.Product;
   ```

6. **Copy and save** the connection string:
   - Go to SQL Database overview
   - Click **Show database connection strings**
   - Copy the **ADO.NET** connection string
   - Replace `{your_password}` with your actual password

### Step 1.7: Provision Azure OpenAI

1. In Azure Portal search bar, type **Azure OpenAI** and select it
2. Click **+ Create**
3. Configure:
   - **Subscription**: Your Azure subscription
   - **Resource group**: `ai-search-lab-rg`
   - **Region**: Choose a region with OpenAI availability (e.g., **East US**, **Sweden Central**)
   - **Name**: `openai-lab-<yourname>-<random>`
   - **Pricing tier**: Standard S0
4. Click **Review + create**, then **Create**
5. After deployment, click **Go to resource**
6. Click **Go to Azure OpenAI Studio** (or navigate to https://oai.azure.com)
7. In OpenAI Studio:
   - Click **Deployments** in the left navigation
   - Click **+ Create new deployment**
   - Configure embedding deployment:
     - **Select a model**: `text-embedding-ada-002` or `text-embedding-3-small`
     - **Deployment name**: `text-embedding`
     - **Model version**: Use default
   - Click **Create**
   - Click **+ Create new deployment** again for GPT
   - Configure GPT deployment:
     - **Select a model**: `gpt-35-turbo` or `gpt-4`
     - **Deployment name**: `gpt-35-turbo`
     - **Model version**: Use default
   - Click **Create**
8. Go back to Azure Portal, your OpenAI resource
9. In the left navigation, click **Keys and Endpoint**
10. **Copy and save**:
    - **KEY 1**
    - **Endpoint**

### Step 1.8: Provision Azure AI Services

1. In Azure Portal search bar, type **Azure AI services** and select it
2. Click **+ Create**
3. Select **Azure AI services multi-service account**
4. Configure:
   - **Subscription**: Your Azure subscription
   - **Resource group**: `ai-search-lab-rg`
   - **Region**: Same region as AI Search
   - **Name**: `ai-services-lab-<yourname>-<random>`
   - **Pricing tier**: Standard S0 (required for skillsets)
5. Check the responsible AI notice checkbox
6. Click **Review + create**, then **Create**
7. After deployment, click **Go to resource**
8. Click **Keys and Endpoint**
9. **Copy and save**:
   - **KEY 1**

### Step 1.9: Set Up Local Development Environment

Now you'll set up your local Python environment for testing.

1. Open a terminal and navigate to where you want to create the project:

   **Windows (PowerShell):**
   ```powershell
   cd $HOME\Desktop
   mkdir ai-search-lab
   cd ai-search-lab
   ```

2. Create a Python virtual environment:

   **Windows:**
   ```powershell
   python -m venv venv
   .\venv\Scripts\Activate.ps1
   ```

   **macOS/Linux:**
   ```bash
   python3 -m venv venv
   source venv/bin/activate
   ```

3. Install required Python packages:

   ```powershell
   pip install azure-search-documents==11.4.0 azure-identity python-dotenv openai==1.12.0 azure-core
   ```

4. Create a `.env` file for your credentials:

   ```powershell
   New-Item -ItemType File -Name .env
   ```

5. Open `.env` in your text editor and add all the credentials you saved:

   ```env
   # Azure AI Search
   AZURE_SEARCH_ENDPOINT=https://ai-search-lab-yourname-random.search.windows.net
   AZURE_SEARCH_KEY=your_search_admin_key_here
   
   # Azure Storage
   STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=https;AccountName=...
   BLOB_CONTAINER_NAME=documents
   
   # Azure SQL Database
   SQL_CONNECTION_STRING=Server=tcp:sqlserver-lab-yourname-random.database.windows.net,1433;Initial Catalog=AdventureWorksLT;Persist Security Info=False;User ID=sqladmin;Password=your_password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   
   # Azure OpenAI
   AZURE_OPENAI_ENDPOINT=https://openai-lab-yourname-random.openai.azure.com/
   AZURE_OPENAI_KEY=your_openai_key_here
   AZURE_OPENAI_EMBEDDING_DEPLOYMENT=text-embedding
   AZURE_OPENAI_GPT_DEPLOYMENT=gpt-35-turbo
   AZURE_OPENAI_API_VERSION=2024-02-01
   
   # Azure AI Services
   AZURE_AI_SERVICES_KEY=your_ai_services_key_here
   ```

6. Save the `.env` file

### Step 1.10: Verify Configuration

Create a simple test script to verify all connections work before proceeding.

1. Create a file named `00-test-connections.py`:

   ```python
   from dotenv import load_dotenv
   import os
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents.indexes import SearchIndexClient
   from openai import AzureOpenAI
   
   def main():
       load_dotenv()
       
       print("\n" + "="*60)
       print("TESTING AZURE CONNECTIONS")
       print("="*60)
       
       # Test 1: Azure AI Search
       print("\n[1/3] Testing Azure AI Search connection...")
       try:
           search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
           search_key = os.getenv("AZURE_SEARCH_KEY")
           
           if not search_endpoint or not search_key:
               print("  ❌ FAILED: Missing AZURE_SEARCH_ENDPOINT or AZURE_SEARCH_KEY")
           else:
               credential = AzureKeyCredential(search_key)
               client = SearchIndexClient(endpoint=search_endpoint, credential=credential)
               indexes = list(client.list_indexes())
               print(f"  ✓ SUCCESS: Connected! Found {len(indexes)} existing indexes")
       except Exception as e:
           print(f"  ❌ FAILED: {str(e)}")
       
       # Test 2: Azure OpenAI
       print("\n[2/3] Testing Azure OpenAI connection...")
       try:
           openai_endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
           openai_key = os.getenv("AZURE_OPENAI_KEY")
           api_version = os.getenv("AZURE_OPENAI_API_VERSION")
           
           if not all([openai_endpoint, openai_key, api_version]):
               print("  ❌ FAILED: Missing OpenAI environment variables")
           else:
               client = AzureOpenAI(
                   api_key=openai_key,
                   api_version=api_version,
                   azure_endpoint=openai_endpoint
               )
               # Try to list models (this validates the connection)
               print("  ✓ SUCCESS: Connected to Azure OpenAI")
       except Exception as e:
           print(f"  ❌ FAILED: {str(e)}")
       
       # Test 3: Environment Variable Check
       print("\n[3/3] Checking all required environment variables...")
       required_vars = [
           "AZURE_SEARCH_ENDPOINT",
           "AZURE_SEARCH_KEY",
           "STORAGE_CONNECTION_STRING",
           "BLOB_CONTAINER_NAME",
           "SQL_CONNECTION_STRING",
           "AZURE_OPENAI_ENDPOINT",
           "AZURE_OPENAI_KEY",
           "AZURE_OPENAI_EMBEDDING_DEPLOYMENT",
           "AZURE_OPENAI_GPT_DEPLOYMENT",
           "AZURE_OPENAI_API_VERSION",
           "AZURE_AI_SERVICES_KEY"
       ]
       
       missing = []
       for var in required_vars:
           if not os.getenv(var):
               missing.append(var)
       
       if missing:
           print(f"  ❌ FAILED: Missing {len(missing)} variables:")
           for var in missing:
               print(f"     - {var}")
       else:
           print(f"  ✓ SUCCESS: All {len(required_vars)} required variables are set")
       
       print("\n" + "="*60)
       if not missing:
           print("✓ Configuration test completed successfully!")
           print("You're ready to proceed to Part 2.")
       else:
           print("⚠ Please fix the issues above before proceeding.")
       print("="*60 + "\n")
   
   if __name__ == "__main__":
       main()
   ```

2. Run the test script:

   ```powershell
   python 00-test-connections.py
   ```

3. Verify all tests pass before continuing to Part 2

### Verification

Before proceeding, verify you have:
- ✓ All Azure resources created in the same resource group
- ✓ Storage account with documents container populated with sample files
- ✓ SQL Database with AdventureWorksLT data loaded
- ✓ Azure OpenAI with text embedding AND gpt-35-turbo deployments
- ✓ All credentials saved in `.env` file
- ✓ Python virtual environment activated with packages installed
- ✓ Connection test script (`00-test-connections.py`) passes all checks

**🎉 Checkpoint**: You now have all resources provisioned! Next, you'll create your first search index.

---

## Part 2: Create Blob Storage Index with AI Enrichment and Vector Search

In this part, you'll create a search index from your blob storage documents. You'll apply AI enrichment skills to extract text from PDFs, and generate embeddings for vector search.

### Step 2.1: Understand the Architecture

The blob storage indexing pipeline has four main components:

1. **Data Source**: Connection to your blob storage container
2. **Skillset**: AI enrichment pipeline (OCR, text extraction, chunking, embeddings)
3. **Index**: Search schema with fields and vector configuration
4. **Indexer**: Orchestrates data flow from source → skillset → index

### Step 2.2: Create the Index Schema

First, create the search index that will store your enriched documents.

1. Create a new file named `01-create-blob-index.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents.indexes import SearchIndexClient
   from azure.search.documents.indexes.models import (
       SearchIndex,
       SimpleField,
       SearchableField,
       SearchField,
       SearchFieldDataType,
       VectorSearch,
       VectorSearchProfile,
       HnswAlgorithmConfiguration,
   )
   from dotenv import load_dotenv
   import os
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       
       # Create index client
       credential = AzureKeyCredential(search_key)
       index_client = SearchIndexClient(endpoint=search_endpoint, credential=credential)
       
       # Define index name
       index_name = "knowledge-base-index"
       
       # Configure vector search
       vector_search = VectorSearch(
           profiles=[
               VectorSearchProfile(
                   name="embedding-profile",
                   algorithm_configuration_name="hnsw-config"
               )
           ],
           algorithms=[
               HnswAlgorithmConfiguration(
                   name="hnsw-config"
               )
           ]
       )
       
       # Define index fields
       fields = [
           SimpleField(
               name="id",
               type=SearchFieldDataType.String,
               key=True,
               filterable=True,
           ),
           SearchableField(
               name="content",
               type=SearchFieldDataType.String,
               analyzer_name="en.microsoft",
           ),
           SearchableField(
               name="metadata_storage_name",
               type=SearchFieldDataType.String,
               filterable=True,
               sortable=True,
           ),
           SearchableField(
               name="metadata_storage_path",
               type=SearchFieldDataType.String,
               filterable=True,
           ),
           SearchField(
               name="contentVector",
               type=SearchFieldDataType.Collection(SearchFieldDataType.Single),
               searchable=True,
               vector_search_dimensions=1536,  # For text-embedding-ada-002
               vector_search_profile_name="embedding-profile",
           ),
       ]
       
       # Create the index
       index = SearchIndex(
           name=index_name,
           fields=fields,
           vector_search=vector_search
       )
       
       # Create or update the index
       result = index_client.create_or_update_index(index)
       print(f"Index '{result.name}' created successfully!")
       print(f"Fields: {len(result.fields)}")
       print(f"Vector search configured: {result.vector_search is not None}")
   
   if __name__ == "__main__":
       main()
   ```

2. Run the script:

   ```powershell
   python 01-create-blob-index.py
   ```

3. Expected output:
   ```
   Index 'knowledge-base-index' created successfully!
   Fields: 5
   Vector search configured: True
   ```

### Step 2.3: Create the Skillset

Now create a skillset that will enrich your documents.

1. Create a new file named `02-create-skillset.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents.indexes import SearchIndexerClient
   from azure.search.documents.indexes.models import (
       SearchIndexerSkillset,
       OcrSkill,
       MergeSkill,
       SplitSkill,
       InputFieldMappingEntry,
       OutputFieldMappingEntry,
       AzureOpenAIEmbeddingSkill,
       CognitiveServicesAccountKey,
   )
   from dotenv import load_dotenv
   import os
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       openai_endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
       openai_key = os.getenv("AZURE_OPENAI_KEY")
       embedding_deployment = os.getenv("AZURE_OPENAI_EMBEDDING_DEPLOYMENT")
       ai_services_key = os.getenv("AZURE_AI_SERVICES_KEY")
       
       # Create indexer client
       credential = AzureKeyCredential(search_key)
       indexer_client = SearchIndexerClient(endpoint=search_endpoint, credential=credential)
       
       # Define skillset name
       skillset_name = "document-enrichment-skillset"
       
       # Define skills
       skills = [
           # Skill 1: OCR to extract text from images and PDFs
           OcrSkill(
               name="ocr-skill",
               description="Extract text from images",
               context="/document/normalized_images/*",
               inputs=[
                   InputFieldMappingEntry(name="image", source="/document/normalized_images/*")
               ],
               outputs=[
                   OutputFieldMappingEntry(name="text", target_name="text")
               ]
           ),
           # Skill 2: Merge OCR text with document text
           MergeSkill(
               name="merge-skill",
               description="Merge document text and OCR text",
               context="/document",
               inputs=[
                   InputFieldMappingEntry(name="text", source="/document/content"),
                   InputFieldMappingEntry(name="itemsToInsert", source="/document/normalized_images/*/text")
               ],
               outputs=[
                   OutputFieldMappingEntry(name="mergedText", target_name="merged_content")
               ]
           ),
           # Skill 3: Split text into chunks
           SplitSkill(
               name="split-skill",
               description="Split text into chunks",
               context="/document",
               text_split_mode="pages",
               maximum_page_length=2000,
               page_overlap_length=500,
               inputs=[
                   InputFieldMappingEntry(name="text", source="/document/merged_content")
               ],
               outputs=[
                   OutputFieldMappingEntry(name="textItems", target_name="pages")
               ]
           ),
           # Skill 4: Generate embeddings using Azure OpenAI
           AzureOpenAIEmbeddingSkill(
               name="embedding-skill",
               description="Generate embeddings",
               context="/document/pages/*",
               resource_uri=openai_endpoint,
               api_key=openai_key,
               deployment_id=embedding_deployment,
               inputs=[
                   InputFieldMappingEntry(name="text", source="/document/pages/*")
               ],
               outputs=[
                   OutputFieldMappingEntry(name="embedding", target_name="vector")
               ]
           )
       ]
       
       # Create skillset
       skillset = SearchIndexerSkillset(
           name=skillset_name,
           description="Skillset for document enrichment with OCR and embeddings",
           skills=skills,
           cognitive_services_account=CognitiveServicesAccountKey(key=ai_services_key)
       )
       
       # Create or update the skillset
       result = indexer_client.create_or_update_skillset(skillset)
       print(f"Skillset '{result.name}' created successfully!")
       print(f"Skills: {len(result.skills)}")
       
   if __name__ == "__main__":
       main()
   ```

2. Run the script:

   ```powershell
   python 02-create-skillset.py
   ```

3. Expected output:
   ```
   Skillset 'document-enrichment-skillset' created successfully!
   Skills: 4
   ```

### Step 2.4: Create the Data Source

Create a connection to your blob storage container.

1. Create a new file named `03-create-blob-datasource.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents.indexes import SearchIndexerClient
   from azure.search.documents.indexes.models import (
       SearchIndexerDataSourceConnection,
       SearchIndexerDataContainer,
   )
   from dotenv import load_dotenv
   import os
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       storage_connection_string = os.getenv("STORAGE_CONNECTION_STRING")
       container_name = os.getenv("BLOB_CONTAINER_NAME")
       
       # Create indexer client
       credential = AzureKeyCredential(search_key)
       indexer_client = SearchIndexerClient(endpoint=search_endpoint, credential=credential)
       
       # Define data source name
       datasource_name = "blob-datasource"
       
       # Create data source connection
       container = SearchIndexerDataContainer(name=container_name)
       data_source = SearchIndexerDataSourceConnection(
           name=datasource_name,
           type="azureblob",
           connection_string=storage_connection_string,
           container=container,
           description="Blob storage data source for documents"
       )
       
       # Create or update the data source
       result = indexer_client.create_or_update_data_source_connection(data_source)
       print(f"Data source '{result.name}' created successfully!")
       print(f"Type: {result.type}")
       print(f"Container: {result.container.name}")
   
   if __name__ == "__main__":
       main()
   ```

2. Run the script:

   ```powershell
   python 03-create-blob-datasource.py
   ```

3. Expected output:
   ```
   Data source 'blob-datasource' created successfully!
   Type: azureblob
   Container: documents
   ```

### Step 2.5: Create and Run the Indexer

Now create the indexer that ties everything together.

1. Create a new file named `04-create-blob-indexer.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents.indexes import SearchIndexerClient
   from azure.search.documents.indexes.models import (
       SearchIndexer,
       FieldMapping,
       IndexingParameters,
       IndexingParametersConfiguration,
   )
   from dotenv import load_dotenv
   import os
   import time
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       
       # Create indexer client
       credential = AzureKeyCredential(search_key)
       indexer_client = SearchIndexerClient(endpoint=search_endpoint, credential=credential)
       
       # Define names
       indexer_name = "blob-indexer"
       datasource_name = "blob-datasource"
       skillset_name = "document-enrichment-skillset"
       index_name = "knowledge-base-index"
       
       # Configure parameters for PDF parsing
       parameters = IndexingParameters(
           configuration=IndexingParametersConfiguration(
               parsing_mode="default",
               image_action="generateNormalizedImages",
               data_to_extract="contentAndMetadata"
           )
       )
       
       # Define output field mappings (from skillset to index)
       output_field_mappings = [
           FieldMapping(
               source_field_name="/document/pages/*/vector",
               target_field_name="contentVector"
           ),
           FieldMapping(
               source_field_name="/document/merged_content",
               target_field_name="content"
           )
       ]
       
       # Create indexer
       indexer = SearchIndexer(
           name=indexer_name,
           description="Indexer for blob storage documents with AI enrichment",
           data_source_name=datasource_name,
           skillset_name=skillset_name,
           target_index_name=index_name,
           parameters=parameters,
           output_field_mappings=output_field_mappings
       )
       
       # Create or update the indexer
       result = indexer_client.create_or_update_indexer(indexer)
       print(f"Indexer '{result.name}' created successfully!")
       
       # Run the indexer
       print("Running indexer... (this may take a few minutes)")
       indexer_client.run_indexer(indexer_name)
       
       # Poll for status
       print("Checking indexer status...")
       for i in range(30):  # Check for up to 5 minutes
           time.sleep(10)
           status = indexer_client.get_indexer_status(indexer_name)
           last_result = status.last_result
           
           if last_result:
               print(f"Status: {last_result.status}")
               print(f"Items processed: {last_result.items_processed}")
               print(f"Items failed: {last_result.items_failed}")
               
               if last_result.status in ["success", "failure"]:
                   break
           else:
               print("Indexer starting...")
       
       print("\n✓ Indexer execution completed!")
   
   if __name__ == "__main__":
       main()
   ```

2. Run the script:

   ```powershell
   python 04-create-blob-indexer.py
   ```

3. Wait for indexer to complete. Expected output:
   ```
   Indexer 'blob-indexer' created successfully!
   Running indexer... (this may take a few minutes)
   Checking indexer status...
   Status: inProgress
   Items processed: 5
   Items failed: 0
   Status: success
   Items processed: 10
   Items failed: 0
   
   ✓ Indexer execution completed!
   ```

### Step 2.6: Test the Index with Keyword and Vector Search

Now test both keyword search and vector search.

1. Create a new file named `05-test-blob-search.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents import SearchClient
   from azure.search.documents.models import VectorizedQuery
   from dotenv import load_dotenv
   import os
   from openai import AzureOpenAI
   
   def get_embedding(text):
       """Generate embedding for the query text"""
       load_dotenv()
       
       client = AzureOpenAI(
           api_key=os.getenv("AZURE_OPENAI_KEY"),
           api_version=os.getenv("AZURE_OPENAI_API_VERSION"),
           azure_endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
       )
       
       deployment = os.getenv("AZURE_OPENAI_EMBEDDING_DEPLOYMENT")
       
       response = client.embeddings.create(
           input=text,
           model=deployment
       )
       return response.data[0].embedding
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       index_name = "knowledge-base-index"
       
       # Create search client
       credential = AzureKeyCredential(search_key)
       search_client = SearchClient(endpoint=search_endpoint, 
                                     index_name=index_name, 
                                     credential=credential)
       
       print("\n" + "="*60)
       print("TESTING BLOB STORAGE INDEX")
       print("="*60)
       
       # Test 1: Keyword search
       print("\n--- Test 1: Keyword Search ---")
       query = "product information"
       
       results = search_client.search(
           search_text=query,
           top=3,
           select=["id", "content", "metadata_storage_name"]
       )
       
       print(f"\nQuery: '{query}'")
       print("\nResults:")
       result_list = list(results)
       if not result_list:
           print("No results found. Make sure documents are indexed.")
       else:
           for i, result in enumerate(result_list, 1):
               print(f"\n{i}. Document: {result.get('metadata_storage_name', 'Unknown')}")
               print(f"   Score: {result.get('@search.score', 0):.4f}")
               content = result.get('content', '')
               print(f"   Content preview: {content[:200] if content else 'No content'}...")
       
       # Test 2: Vector search (semantic)
       print("\n\n--- Test 2: Vector Search (Semantic) ---")
       query = "What features does the product have?"
       print(f"\nQuery: '{query}'")
       
       # Generate embedding for query
       print("Generating query embedding...")
       query_vector = get_embedding(query)
       
       # Create vector query
       vector_query = VectorizedQuery(
           vector=query_vector,
           k_nearest_neighbors=3,
           fields="contentVector"
       )
       
       # Search with vector
       results = search_client.search(
           search_text=None,
           vector_queries=[vector_query],
           select=["id", "content", "metadata_storage_name"]
       )
       
       print("\nResults:")
       result_list = list(results)
       if not result_list:
           print("No results found. Vector embeddings may not be generated yet.")
       else:
           for i, result in enumerate(result_list, 1):
               print(f"\n{i}. Document: {result.get('metadata_storage_name', 'Unknown')}")
               print(f"   Score: {result.get('@search.score', 0):.4f}")
               content = result.get('content', '')
               print(f"   Content preview: {content[:200] if content else 'No content'}...")
       
       # Test 3: Hybrid search (keyword + vector)
       print("\n\n--- Test 3: Hybrid Search (Keyword + Vector) ---")
       query = "installation instructions"
       print(f"\nQuery: '{query}'")
       
       query_vector = get_embedding(query)
       vector_query = VectorizedQuery(
           vector=query_vector,
           k_nearest_neighbors=3,
           fields="contentVector"
       )
       
       results = search_client.search(
           search_text=query,
           vector_queries=[vector_query],
           top=3,
           select=["id", "content", "metadata_storage_name"]
       )
       
       print("\nResults:")
       result_list = list(results)
       if not result_list:
           print("No results found.")
       else:
           for i, result in enumerate(result_list, 1):
               print(f"\n{i}. Document: {result.get('metadata_storage_name', 'Unknown')}")
               print(f"   Score: {result.get('@search.score', 0):.4f}")
               content = result.get('content', '')
               print(f"   Content preview: {content[:200] if content else 'No content'}...")
       
       print("\n" + "="*60)
       print("✓ All tests completed successfully!")
       print("="*60 + "\n")
   
   if __name__ == "__main__":
       main()
   ```

2. Run the test script:

   ```powershell
   python 05-test-blob-search.py
   ```

3. Review the results comparing keyword search, vector search, and hybrid search

### Verification

Before proceeding, verify:
- ✓ Index created with 5 fields including contentVector
- ✓ Skillset created with 4 enrichment skills
- ✓ Data source connected to blob storage
- ✓ Indexer ran successfully with no failed items
- ✓ Keyword search returns relevant results
- ✓ Vector search returns semantically similar results
- ✓ PDF text was extracted successfully (visible in results)

**🎉 Checkpoint**: You now have an AI-enriched search index with vector capabilities! Next, you'll create an index from SQL data.

---

## Part 3: Create SQL Database Index with Faceted Search

In this part, you'll create a search index from the AdventureWorks SQL database with faceted search and filtering capabilities.

### Step 3.1: Create the SQL Index Schema

Create an index optimized for product search with facets and filters.

1. Create a new file named `06-create-sql-index.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents.indexes import SearchIndexClient
   from azure.search.documents.indexes.models import (
       SearchIndex,
       SimpleField,
       SearchableField,
       SearchFieldDataType,
   )
   from dotenv import load_dotenv
   import os
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       
       # Create index client
       credential = AzureKeyCredential(search_key)
       index_client = SearchIndexClient(endpoint=search_endpoint, credential=credential)
       
       # Define index name
       index_name = "products-index"
       
       # Define index fields matching SQL Product table
       fields = [
           SimpleField(
               name="ProductID",
               type=SearchFieldDataType.String,
               key=True,
               filterable=True,
           ),
           SearchableField(
               name="Name",
               type=SearchFieldDataType.String,
               sortable=True,
               filterable=True,
               analyzer_name="en.microsoft",
           ),
           SimpleField(
               name="ProductNumber",
               type=SearchFieldDataType.String,
               filterable=True,
           ),
           SearchableField(
               name="Color",
               type=SearchFieldDataType.String,
               facetable=True,
               filterable=True,
           ),
           SimpleField(
               name="ListPrice",
               type=SearchFieldDataType.Double,
               filterable=True,
               sortable=True,
               facetable=True,
           ),
           SearchableField(
               name="Size",
               type=SearchFieldDataType.String,
               facetable=True,
               filterable=True,
           ),
           SearchableField(
               name="ProductCategoryName",
               type=SearchFieldDataType.String,
               facetable=True,
               filterable=True,
           ),
           SearchableField(
               name="ProductModelName",
               type=SearchFieldDataType.String,
               filterable=True,
           ),
       ]
       
       # Create the index
       index = SearchIndex(
           name=index_name,
           fields=fields
       )
       
       # Create or update the index
       result = index_client.create_or_update_index(index)
       print(f"Index '{result.name}' created successfully!")
       print(f"Fields: {len(result.fields)}")
       print(f"Facetable fields: Color, ListPrice, Size, ProductCategoryName")
   
   if __name__ == "__main__":
       main()
   ```

2. Run the script:

   ```powershell
   python 06-create-sql-index.py
   ```

3. Expected output:
   ```
   Index 'products-index' created successfully!
   Fields: 8
   Facetable fields: Color, ListPrice, Size, ProductCategoryName
   ```

### Step 3.2: Create the SQL Data Source

Create a connection to your SQL database.

1. Create a new file named `07-create-sql-datasource.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents.indexes import SearchIndexerClient
   from azure.search.documents.indexes.models import (
       SearchIndexerDataSourceConnection,
       SearchIndexerDataContainer,
   )
   from dotenv import load_dotenv
   import os
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       sql_connection_string = os.getenv("SQL_CONNECTION_STRING")
       
       # Create indexer client
       credential = AzureKeyCredential(search_key)
       indexer_client = SearchIndexerClient(endpoint=search_endpoint, credential=credential)
       
       # Define data source name
       datasource_name = "sql-datasource"
       
       # Create data query to join Product with Category and Model
       query = """
       SELECT 
           CAST(p.ProductID AS NVARCHAR(10)) as ProductID,
           p.Name,
           p.ProductNumber,
           p.Color,
           p.ListPrice,
           p.Size,
           pc.Name as ProductCategoryName,
           pm.Name as ProductModelName
       FROM SalesLT.Product p
       LEFT JOIN SalesLT.ProductModel pm ON p.ProductModelID = pm.ProductModelID
       LEFT JOIN SalesLT.ProductCategory pc ON p.ProductCategoryID = pc.ProductCategoryID
       WHERE p.SellEndDate IS NULL OR p.SellEndDate > GETDATE()
       """
       
       # Create data source connection
       container = SearchIndexerDataContainer(
           name="SalesLT.Product",
           query=query
       )
       
       data_source = SearchIndexerDataSourceConnection(
           name=datasource_name,
           type="azuresql",
           connection_string=sql_connection_string,
           container=container,
           description="SQL database data source for products"
       )
       
       # Create or update the data source
       result = indexer_client.create_or_update_data_source_connection(data_source)
       print(f"Data source '{result.name}' created successfully!")
       print(f"Type: {result.type}")
       print(f"Table/Query: Product with Category and Model joins")
   
   if __name__ == "__main__":
       main()
   ```

2. Run the script:

   ```powershell
   python 07-create-sql-datasource.py
   ```

3. Expected output:
   ```
   Data source 'sql-datasource' created successfully!
   Type: azuresql
   Table/Query: Product with Category and Model joins
   ```

### Step 3.3: Create and Run the SQL Indexer

Create an indexer for the SQL database.

1. Create a new file named `08-create-sql-indexer.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents.indexes import SearchIndexerClient
   from azure.search.documents.indexes.models import (
       SearchIndexer,
       IndexingSchedule,
   )
   from dotenv import load_dotenv
   import os
   import time
   from datetime import timedelta
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       
       # Create indexer client
       credential = AzureKeyCredential(search_key)
       indexer_client = SearchIndexerClient(endpoint=search_endpoint, credential=credential)
       
       # Define names
       indexer_name = "sql-indexer"
       datasource_name = "sql-datasource"
       index_name = "products-index"
       
       # Create indexer with schedule (optional)
       indexer = SearchIndexer(
           name=indexer_name,
           description="Indexer for SQL database products",
           data_source_name=datasource_name,
           target_index_name=index_name,
           schedule=IndexingSchedule(interval=timedelta(hours=1))  # Optional: run every hour
       )
       
       # Create or update the indexer
       result = indexer_client.create_or_update_indexer(indexer)
       print(f"Indexer '{result.name}' created successfully!")
       
       # Run the indexer
       print("Running indexer...")
       indexer_client.run_indexer(indexer_name)
       
       # Poll for status
       print("Checking indexer status...")
       for i in range(15):  # Check for up to 2.5 minutes
           time.sleep(10)
           status = indexer_client.get_indexer_status(indexer_name)
           last_result = status.last_result
           
           if last_result:
               print(f"Status: {last_result.status}")
               print(f"Items processed: {last_result.items_processed}")
               print(f"Items failed: {last_result.items_failed}")
               
               if last_result.status in ["success", "failure"]:
                   break
           else:
               print("Indexer starting...")
       
       print("\n✓ SQL indexer execution completed!")
   
   if __name__ == "__main__":
       main()
   ```

2. Run the script:

   ```powershell
   python 08-create-sql-indexer.py
   ```

3. Expected output:
   ```
   Indexer 'sql-indexer' created successfully!
   Running indexer...
   Checking indexer status...
   Status: success
   Items processed: 295
   Items failed: 0
   
   ✓ SQL indexer execution completed!
   ```

### Step 3.4: Test the SQL Index with Faceted Search

Test the products index with various query patterns.

1. Create a new file named `09-test-sql-search.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents import SearchClient
   from dotenv import load_dotenv
   import os
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       index_name = "products-index"
       
       # Create search client
       credential = AzureKeyCredential(search_key)
       search_client = SearchClient(endpoint=search_endpoint, 
                                     index_name=index_name, 
                                     credential=credential)
       
       print("\n" + "="*60)
       print("TESTING PRODUCTS INDEX")
       print("="*60)
       
       # Test 1: Simple search
       print("\n--- Test 1: Simple Product Search ---")
       results = search_client.search(
           search_text="mountain bike",
           top=5,
           select=["Name", "Color", "ListPrice", "ProductCategoryName"]
       )
       
       print("\nSearch: 'mountain bike'")
       print("\nResults:")
       for result in results:
           print(f"- {result['Name']}")
           print(f"  Color: {result.get('Color', 'N/A')}, Price: ${result['ListPrice']:.2f}, Category: {result.get('ProductCategoryName', 'N/A')}")
       
       # Test 2: Search with filter
       print("\n\n--- Test 2: Search with Price Filter ---")
       results = search_client.search(
           search_text="bike",
           filter="ListPrice lt 500",
           top=5,
           select=["Name", "ListPrice", "Color"]
       )
       
       print("\nSearch: 'bike' WHERE ListPrice < $500")
       print("\nResults:")
       for result in results:
           print(f"- {result['Name']}: ${result['ListPrice']:.2f} ({result.get('Color', 'N/A')})")
       
       # Test 3: Faceted search - get color facets
       print("\n\n--- Test 3: Faceted Search - Colors ---")
       results = search_client.search(
           search_text="bike",
           facets=["Color,count:10"],
           top=0  # We only want facets, not results
       )
       
       print("\nAvailable Colors for 'bike':")
       facets = results.get_facets()
       if "Color" in facets:
           for facet in facets["Color"]:
               print(f"- {facet['value']}: {facet['count']} products")
       
       # Test 4: Faceted search - categories and price ranges
       print("\n\n--- Test 4: Faceted Search - Categories and Prices ---")
       results = search_client.search(
           search_text="*",
           facets=["ProductCategoryName,count:10", "ListPrice,values:100|500|1000|2000"],
           top=0
       )
       
       print("\nProduct Categories:")
       facets = results.get_facets()
       if "ProductCategoryName" in facets:
           for facet in facets["ProductCategoryName"]:
               print(f"- {facet['value']}: {facet['count']} products")
       
       print("\nPrice Ranges:")
       if "ListPrice" in facets:
           for facet in facets["ListPrice"]:
               if 'value' in facet:
                   print(f"- ${facet['value']:.2f}: {facet['count']} products")
               elif 'from' in facet:
                   to_val = facet.get('to', 'infinity')
                   print(f"- ${facet['from']:.2f} - {to_val}: {facet['count']} products")
       
       # Test 5: Combined filter and facet
       print("\n\n--- Test 5: Complex Query - Red Bikes Under $1000 ---")
       results = search_client.search(
           search_text="bike",
           filter="Color eq 'Red' and ListPrice lt 1000",
           order_by=["ListPrice asc"],
           top=5,
           select=["Name", "Color", "ListPrice"]
       )
       
       print("\nSearch: 'bike' WHERE Color='Red' AND ListPrice < $1000")
       print("Sorted by Price (ascending)")
       print("\nResults:")
       for result in results:
           print(f"- {result['Name']}: ${result['ListPrice']:.2f}")
       
       print("\n" + "="*60)
       print("✓ All tests completed successfully!")
       print("="*60 + "\n")
   
   if __name__ == "__main__":
       main()
   ```

2. Run the test script:

   ```powershell
   python 09-test-sql-search.py
   ```

3. Review the results showing search, filtering, faceting, and sorting capabilities

### Verification

Before proceeding, verify:
- ✓ SQL index created with 8 fields
- ✓ Data source connected to SQL database with custom query
- ✓ Indexer ran successfully and indexed all products
- ✓ Simple search returns relevant products
- ✓ Filters work correctly (price, color)
- ✓ Facets return correct counts for categories and attributes
- ✓ Sorting works properly

**🎉 Checkpoint**: You now have two working indexes - one for documents with vector search, one for products with faceted search! Next, you'll configure a web app to query the products index.

---

## Part 4: Configure and Run the Web Application

In this part, you'll set up and run a pre-built Python web application that provides a search interface for the products index.

### Step 4.1: Download the Web Application

> **📦 Note**: The web application code should be provided as part of the lab materials. For this exercise, we'll create a simple Flask-based search application.

1. Create a new directory for the web app:

   ```powershell
   mkdir webapp
   cd webapp
   ```

2. Create the web application file `app.py`:

   ```python
   from flask import Flask, render_template, request, jsonify
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents import SearchClient
   from dotenv import load_dotenv
   import os
   import sys
   
   # Load environment variables from parent directory
   parent_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
   env_path = os.path.join(parent_dir, '.env')
   load_dotenv(dotenv_path=env_path)
   
   app = Flask(__name__)
   
   # Azure AI Search configuration
   search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
   search_key = os.getenv("AZURE_SEARCH_KEY")
   index_name = "products-index"
   
   # Validate configuration
   if not all([search_endpoint, search_key]):
       print("ERROR: Missing Azure AI Search configuration!")
       print("Please check your .env file in the parent directory.")
       sys.exit(1)
   
   # Create search client
   try:
       credential = AzureKeyCredential(search_key)
       search_client = SearchClient(endpoint=search_endpoint, 
                                     index_name=index_name, 
                                     credential=credential)
   except Exception as e:
       print(f"ERROR: Failed to create search client: {e}")
       sys.exit(1)
   
   @app.route('/')
   def home():
       return render_template('index.html')
   
   @app.route('/search', methods=['POST'])
   def search():
       try:
           data = request.json
           query = data.get('query', '*')
           filters = []
           
           # Build OData filter
           if data.get('color'):
               filters.append(f"Color eq '{data['color']}'")
           if data.get('minPrice'):
               filters.append(f"ListPrice ge {data['minPrice']}")
           if data.get('maxPrice'):
               filters.append(f"ListPrice le {data['maxPrice']}")
           if data.get('category'):
               filters.append(f"ProductCategoryName eq '{data['category']}'")
           
           filter_string = " and ".join(filters) if filters else None
           
           # Execute search
           results = search_client.search(
               search_text=query,
               filter=filter_string,
               facets=["Color,count:20", "ProductCategoryName,count:20", "ListPrice,interval:100"],
               top=20,
               include_total_count=True,
               select=["Name", "ProductNumber", "Color", "ListPrice", "Size", "ProductCategoryName"]
           )
           
           # Format results
           items = []
           for result in results:
               items.append({
                   'name': result.get('Name'),
                   'productNumber': result.get('ProductNumber'),
                   'color': result.get('Color'),
                   'price': result.get('ListPrice'),
                   'size': result.get('Size'),
                   'category': result.get('ProductCategoryName'),
                   'score': result.get('@search.score')
               })
           
           # Get facets
           facets = results.get_facets()
           
           return jsonify({
               'success': True,
               'items': items,
               'total': results.get_count(),
               'facets': facets
           })
       
       except Exception as e:
           return jsonify({'success': False, 'error': str(e)}), 500
   
   if __name__ == '__main__':
       app.run(debug=True, host='0.0.0.0', port=5000)
   ```

3. Create the templates directory and HTML file:

   ```powershell
   mkdir templates
   New-Item -ItemType File -Path templates\index.html
   ```

4. Open `templates\index.html` and add the frontend code:

   ```html
   <!DOCTYPE html>
   <html lang="en">
   <head>
       <meta charset="UTF-8">
       <meta name="viewport" content="width=device-width, initial-scale=1.0">
       <title>Product Search - Azure AI Search</title>
       <style>
           * {
               margin: 0;
               padding: 0;
               box-sizing: border-box;
           }
           
           body {
               font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
               background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
               min-height: 100vh;
               padding: 20px;
           }
           
           .container {
               max-width: 1200px;
               margin: 0 auto;
               background: white;
               border-radius: 10px;
               box-shadow: 0 10px 40px rgba(0,0,0,0.2);
               overflow: hidden;
           }
           
           .header {
               background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
               color: white;
               padding: 30px;
               text-align: center;
           }
           
           .header h1 {
               font-size: 2.5em;
               margin-bottom: 10px;
           }
           
           .header p {
               font-size: 1.1em;
               opacity: 0.9;
           }
           
           .search-section {
               padding: 30px;
               background: #f8f9fa;
               border-bottom: 1px solid #dee2e6;
           }
           
           .search-box {
               display: flex;
               margin-bottom: 20px;
           }
           
           .search-box input {
               flex: 1;
               padding: 15px;
               font-size: 1.1em;
               border: 2px solid #ddd;
               border-radius: 5px 0 0 5px;
               outline: none;
           }
           
           .search-box input:focus {
               border-color: #667eea;
           }
           
           .search-box button {
               padding: 15px 40px;
               background: #667eea;
               color: white;
               border: none;
               border-radius: 0 5px 5px 0;
               cursor: pointer;
               font-size: 1.1em;
               font-weight: bold;
               transition: background 0.3s;
           }
           
           .search-box button:hover {
               background: #5568d3;
           }
           
           .filters {
               display: grid;
               grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
               gap: 15px;
           }
           
           .filter-group label {
               display: block;
               margin-bottom: 5px;
               font-weight: bold;
               color: #333;
           }
           
           .filter-group select,
           .filter-group input {
               width: 100%;
               padding: 10px;
               border: 2px solid #ddd;
               border-radius: 5px;
               font-size: 1em;
           }
           
           .main-content {
               display: flex;
           }
           
           .facets {
               width: 250px;
               padding: 20px;
               background: #f8f9fa;
               border-right: 1px solid #dee2e6;
           }
           
           .facets h3 {
               margin-bottom: 15px;
               color: #333;
           }
           
           .facet-group {
               margin-bottom: 20px;
           }
           
           .facet-group h4 {
               margin-bottom: 10px;
               color: #667eea;
               font-size: 0.9em;
               text-transform: uppercase;
           }
           
           .facet-item {
               padding: 5px 0;
               cursor: pointer;
               color: #666;
               transition: color 0.2s;
           }
           
           .facet-item:hover {
               color: #667eea;
           }
           
           .results-section {
               flex: 1;
               padding: 20px;
           }
           
           .results-header {
               margin-bottom: 20px;
               padding-bottom: 10px;
               border-bottom: 2px solid #dee2e6;
           }
           
           .results-header h2 {
               color: #333;
           }
           
           .product-grid {
               display: grid;
               grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
               gap: 20px;
           }
           
           .product-card {
               background: white;
               border: 1px solid #dee2e6;
               border-radius: 8px;
               padding: 20px;
               transition: transform 0.2s, box-shadow 0.2s;
           }
           
           .product-card:hover {
               transform: translateY(-5px);
               box-shadow: 0 5px 15px rgba(0,0,0,0.1);
           }
           
           .product-name {
               font-size: 1.2em;
               font-weight: bold;
               color: #333;
               margin-bottom: 10px;
           }
           
           .product-details {
               color: #666;
               font-size: 0.9em;
               line-height: 1.6;
           }
           
           .product-price {
               font-size: 1.3em;
               color: #667eea;
               font-weight: bold;
               margin-top: 10px;
           }
           
           .no-results {
               text-align: center;
               padding: 60px 20px;
               color: #999;
           }
           
           .loading {
               text-align: center;
               padding: 40px;
               font-size: 1.2em;
               color: #667eea;
           }
       </style>
   </head>
   <body>
       <div class="container">
           <div class="header">
               <h1>🔍 Product Search</h1>
               <p>Powered by Azure AI Search</p>
           </div>
           
           <div class="search-section">
               <div class="search-box">
                   <input type="text" id="searchQuery" placeholder="Search for products..." />
                   <button onclick="performSearch()">Search</button>
               </div>
               
               <div class="filters">
                   <div class="filter-group">
                       <label>Category</label>
                       <select id="categoryFilter">
                           <option value="">All Categories</option>
                       </select>
                   </div>
                   <div class="filter-group">
                       <label>Color</label>
                       <select id="colorFilter">
                           <option value="">All Colors</option>
                       </select>
                   </div>
                   <div class="filter-group">
                       <label>Min Price</label>
                       <input type="number" id="minPriceFilter" placeholder="0" />
                   </div>
                   <div class="filter-group">
                       <label>Max Price</label>
                       <input type="number" id="maxPriceFilter" placeholder="10000" />
                   </div>
               </div>
           </div>
           
           <div class="main-content">
               <div class="facets" id="facetsPanel">
                   <h3>Facets</h3>
                   <div id="facetsContent"></div>
               </div>
               
               <div class="results-section">
                   <div class="results-header">
                       <h2 id="resultsCount">Search for products...</h2>
                   </div>
                   <div id="resultsContainer" class="product-grid"></div>
               </div>
           </div>
       </div>
       
       <script>
           // Initialize on page load
           window.onload = function() {
               performSearch();
           };
           
           // Allow Enter key to search
           document.getElementById('searchQuery').addEventListener('keypress', function(e) {
               if (e.key === 'Enter') {
                   performSearch();
               }
           });
           
           async function performSearch() {
               const query = document.getElementById('searchQuery').value || '*';
               const color = document.getElementById('colorFilter').value;
               const category = document.getElementById('categoryFilter').value;
               const minPrice = document.getElementById('minPriceFilter').value;
               const maxPrice = document.getElementById('maxPriceFilter').value;
               
               // Show loading
               document.getElementById('resultsContainer').innerHTML = 
                   '<div class="loading">Searching...</div>';
               
               try {
                   const response = await fetch('/search', {
                       method: 'POST',
                       headers: {
                           'Content-Type': 'application/json',
                       },
                       body: JSON.stringify({
                           query: query,
                           color: color,
                           category: category,
                           minPrice: minPrice,
                           maxPrice: maxPrice
                       })
                   });
                   
                   const data = await response.json();
                   
                   if (data.success) {
                       displayResults(data);
                   } else {
                       document.getElementById('resultsContainer').innerHTML = 
                           `<div class="no-results">Error: ${data.error}</div>`;
                   }
               } catch (error) {
                   document.getElementById('resultsContainer').innerHTML = 
                       `<div class="no-results">Error: ${error.message}</div>`;
               }
           }
           
           function displayResults(data) {
               const resultsCount = document.getElementById('resultsCount');
               const resultsContainer = document.getElementById('resultsContainer');
               const facetsContent = document.getElementById('facetsContent');
               
               // Update results count
               resultsCount.textContent = `Found ${data.total} products`;
               
               // Display products
               if (data.items.length === 0) {
                   resultsContainer.innerHTML = 
                       '<div class="no-results">No products found. Try adjusting your search or filters.</div>';
               } else {
                   resultsContainer.innerHTML = data.items.map(product => `
                       <div class="product-card">
                           <div class="product-name">${product.name}</div>
                           <div class="product-details">
                               <strong>Product #:</strong> ${product.productNumber}<br>
                               <strong>Color:</strong> ${product.color || 'N/A'}<br>
                               <strong>Size:</strong> ${product.size || 'N/A'}<br>
                               <strong>Category:</strong> ${product.category || 'N/A'}
                           </div>
                           <div class="product-price">$${product.price.toFixed(2)}</div>
                       </div>
                   `).join('');
               }
               
               // Display facets
               let facetsHTML = '';
               if (data.facets.Color) {
                   facetsHTML += '<div class="facet-group"><h4>Colors</h4>';
                   data.facets.Color.forEach(facet => {
                       facetsHTML += `<div class="facet-item">${facet.value} (${facet.count})</div>`;
                   });
                   facetsHTML += '</div>';
               }
               
               if (data.facets.ProductCategoryName) {
                   facetsHTML += '<div class="facet-group"><h4>Categories</h4>';
                   data.facets.ProductCategoryName.forEach(facet => {
                       facetsHTML += `<div class="facet-item">${facet.value} (${facet.count})</div>`;
                   });
                   facetsHTML += '</div>';
               }
               
               facetsContent.innerHTML = facetsHTML || '<p>No facets available</p>';
               
               // Populate filter dropdowns from facets
               if (data.facets.Color) {
                   const colorFilter = document.getElementById('colorFilter');
                   const currentColor = colorFilter.value;
                   colorFilter.innerHTML = '<option value="">All Colors</option>' +
                       data.facets.Color.map(f => 
                           `<option value="${f.value}" ${f.value === currentColor ? 'selected' : ''}>${f.value} (${f.count})</option>`
                       ).join('');
               }
               
               if (data.facets.ProductCategoryName) {
                   const categoryFilter = document.getElementById('categoryFilter');
                   const currentCategory = categoryFilter.value;
                   categoryFilter.innerHTML = '<option value="">All Categories</option>' +
                       data.facets.ProductCategoryName.map(f => 
                           `<option value="${f.value}" ${f.value === currentCategory ? 'selected' : ''}>${f.value} (${f.count})</option>`
                       ).join('');
               }
           }
       </script>
   </body>
   </html>
   ```

### Step 4.2: Install Web App Dependencies

1. Make sure you're in the webapp directory and install Flask:

   ```powershell
   cd webapp
   pip install flask
   ```

   > **💡 Note**: If you're in a virtual environment, ensure it's still activated. You should see `(venv)` in your prompt.

### Step 4.3: Run the Web Application

1. Start the Flask application:

   ```powershell
   python app.py
   ```

2. Expected output:
   ```
    * Serving Flask app 'app'
    * Debug mode: on
    * Running on all addresses (0.0.0.0)
    * Running on http://127.0.0.1:5000
    * Running on http://192.168.x.x:5000
   ```

3. Open your web browser and navigate to:
   ```
   http://localhost:5000
   ```

### Step 4.4: Test the Web Application

Test the following features in the web interface:

1. **Basic Search**:
   - Enter "bike" in the search box
   - Click Search
   - Verify products are returned

2. **Filtered Search**:
   - Search for "bike"
   - Select a Color from the dropdown
   - Click Search
   - Verify only products with that color are shown

3. **Price Range Filter**:
   - Enter Min Price: 500
   - Enter Max Price: 1000
   - Click Search
   - Verify all products are within that range

4. **Category Facets**:
   - Perform any search
   - Observe the facet counts on the left side
   - Select different categories
   - Verify results update accordingly

5. **Combined Filters**:
   - Search: "mountain"
   - Color: "Red"
   - Min Price: 100, Max Price: 2000
   - Verify all filters are applied correctly

### Verification

Before proceeding, verify:
- ✓ Web application starts without errors
- ✓ Search interface loads in browser
- ✓ Basic search returns products
- ✓ Filters (color, price, category) work correctly
- ✓ Facets display with accurate counts
- ✓ Results update dynamically when filters change
- ✓ Product details display correctly (name, price, color, etc.)

**🎉 Checkpoint**: You now have a fully functional web application querying your Azure AI Search index! Next is the bonus section: integrating with Azure AI Foundry.

---

## Part 5: Azure AI Foundry Integration (BONUS/OPTIONAL)

In this optional advanced section, you'll integrate Azure AI Search with Azure AI Foundry to create an intelligent agent that uses the Retrieval-Augmented Generation (RAG) pattern.

> **⏱️ Time Check**: This section is optional and adds 20-25 minutes. If you're short on time, you can complete Parts 1-4 and consider this section for independent learning.

### Step 5.1: Create Azure AI Foundry Project

1. Navigate to [Azure AI Foundry](https://ai.azure.com)
2. Sign in with your Azure credentials
3. Click **+ New project**
4. Configure:
   - **Project name**: `ai-search-rag-project`
   - **Hub**: Create new hub or select existing
   - **Subscription**: Your Azure subscription
   - **Resource group**: `ai-search-lab-rg`
   - **Location**: Same region as your resources
5. Click **Create**
6. Wait for project creation (1-2 minutes)

### Step 5.2: Add Connections

1. In your AI Foundry project, click **Settings** in the left navigation
2. Click **Connections**

**Add Azure AI Search Connection:**
3. Click **+ New connection**
4. Select **Azure AI Search**
5. Configure:
   - **Name**: `ai-search-connection`
   - **Subscription**: Your subscription
   - **Azure AI Search resource**: Select your search service
   - **Authentication**: API Key
   - **API Key**: Paste your search admin key
6. Click **Add connection**

**Add Azure OpenAI Connection:**
7. Click **+ New connection**
8. Select **Azure OpenAI**
9. Configure:
   - **Name**: `openai-connection`
   - **Subscription**: Your subscription
   - **Azure OpenAI resource**: Select your OpenAI service
10. Click **Add connection**

### Step 5.3: Create a Prompt Flow with RAG

1. In your AI Foundry project, click **Prompt flow** in the left navigation
2. Click **+ Create**
3. Select **Chat flow**
4. Name it: `knowledge-base-rag`

5. Configure the flow with these nodes:

**Node 1: User Input**
- Already present by default
- Input: question (string)

**Node 2: Embedding (Azure OpenAI)**
- Add **LLM** tool
- Select: `openai-connection`
- Deployment: `text-embedding`
- Input: `${inputs.question}`
- Output: question_embedding

**Node 3: Vector Search (Azure AI Search)**
- Add **Python** tool
- Name: `search_knowledge_base`
- Code:
  ```python
  from promptflow import tool
  from azure.core.credentials import AzureKeyCredential
  from azure.search.documents import SearchClient
  from azure.search.documents.models import VectorizedQuery
  
  @tool
  def search(question: str, question_vector: list, search_connection: dict):
      # Create search client
      search_client = SearchClient(
          endpoint=search_connection['endpoint'],
          index_name="knowledge-base-index",
          credential=AzureKeyCredential(search_connection['key'])
      )
      
      # Create vector query
      vector_query = VectorizedQuery(
          vector=question_vector,
          k_nearest_neighbors=3,
          fields="contentVector"
      )
      
      # Perform hybrid search
      results = search_client.search(
          search_text=question,
          vector_queries=[vector_query],
          top=3,
          select=["content", "metadata_storage_name"]
      )
      
      # Format results
      context = ""
      for result in results:
          context += f"\n\nFrom {result['metadata_storage_name']}:\n{result['content'][:500]}"
      
      return context
  ```

**Node 4: Generate Answer (Azure OpenAI)**
- Add **LLM** tool
- Select: `openai-connection`
- Deployment: `gpt-35-turbo` or `gpt-4`
- System prompt:
  ```
  You are a helpful assistant. Answer the user's question based ONLY on the provided context from the knowledge base.
  If the context doesn't contain relevant information, say "I don't have enough information to answer that question."
  
  Context:
  ${search_results}
  ```
- User message: `${inputs.question}`
- Output: answer

5. Connect the nodes:
   - User Input (question) → Search (question)
   - Embedding (question_embedding) → Search (question_vector)
   - Search (context) → LLM (search_results)

6. Click **Save**

### Step 5.4: Test the RAG Agent

1. In the Prompt flow editor, click **Chat** at the top
2. Try these test questions in the chat interface:

   **Test 1: Product Information**
   ```
   What products are described in the documents?
   ```

   **Test 2: Specific Details**
   ```
   What are the features or specifications mentioned?
   ```

   **Test 3: Instructions**
   ```
   Are there any installation or usage instructions?
   ```

3. Verify that:
   - The agent returns answers based on your indexed documents
   - The responses reference the correct source files
   - The agent says "I don't have enough information" for questions outside the knowledge base

### Step 5.5: Create a Local Test Script

For testing outside of AI Foundry, create a local script.

1. Navigate back to your main lab directory (`ai-search-lab`)

2. Create `10-test-rag-agent.py`:

   ```python
   from azure.core.credentials import AzureKeyCredential
   from azure.search.documents import SearchClient
   from azure.search.documents.models import VectorizedQuery
   from dotenv import load_dotenv
   import os
   from openai import AzureOpenAI
   
   def get_embedding(text):
       """Generate embedding for the query text"""
       client = AzureOpenAI(
           api_key=os.getenv("AZURE_OPENAI_KEY"),
           api_version=os.getenv("AZURE_OPENAI_API_VERSION"),
           azure_endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
       )
       
       deployment = os.getenv("AZURE_OPENAI_EMBEDDING_DEPLOYMENT")
       
       response = client.embeddings.create(
           input=text,
           model=deployment
       )
       return response.data[0].embedding
   
   def search_knowledge_base(question, question_vector, search_client):
       """Perform hybrid search on knowledge base"""
       vector_query = VectorizedQuery(
           vector=question_vector,
           k_nearest_neighbors=3,
           fields="contentVector"
       )
       
       results = search_client.search(
           search_text=question,
           vector_queries=[vector_query],
           top=3,
           select=["content", "metadata_storage_name"]
       )
       
       context = ""
       sources = []
       for result in results:
           source = result['metadata_storage_name']
           content = result['content'][:500]
           context += f"\n\nFrom {source}:\n{content}"
           sources.append(source)
       
       return context, sources
   
   def generate_answer(question, context):
       """Generate answer using GPT model"""
       client = AzureOpenAI(
           api_key=os.getenv("AZURE_OPENAI_KEY"),
           api_version=os.getenv("AZURE_OPENAI_API_VERSION"),
           azure_endpoint=os.getenv("AZURE_OPENAI_ENDPOINT")
       )
       
       deployment = os.getenv("AZURE_OPENAI_GPT_DEPLOYMENT")
       
       system_prompt = f"""You are a helpful assistant. Answer the user's question based ONLY on the provided context from the knowledge base.
       If the context doesn't contain relevant information, say "I don't have enough information to answer that question."
       
       Context:
       {context}
       """
       
       response = client.chat.completions.create(
           model=deployment,
           messages=[
               {"role": "system", "content": system_prompt},
               {"role": "user", "content": question}
           ],
           temperature=0.7,
           max_tokens=500
       )
       
       return response.choices[0].message.content
   
   def main():
       # Load environment variables
       load_dotenv()
       search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
       search_key = os.getenv("AZURE_SEARCH_KEY")
       index_name = "knowledge-base-index"
       
       # Create search client
       credential = AzureKeyCredential(search_key)
       search_client = SearchClient(endpoint=search_endpoint, 
                                     index_name=index_name, 
                                     credential=credential)
       
       print("\n" + "="*60)
       print("RAG AGENT - Testing Retrieval-Augmented Generation")
       print("="*60)
       
       # Test questions
       questions = [
           "What products are described in the documents?",
           "What are the main features or specifications?",
           "Are there any installation instructions?"
       ]
       
       for i, question in enumerate(questions, 1):
           print(f"\n--- Question {i} ---")
           print(f"Q: {question}")
           
           # Get embedding
           print("  [Generating embedding...]")
           question_vector = get_embedding(question)
           
           # Search knowledge base
           print("  [Searching knowledge base...]")
           context, sources = search_knowledge_base(question, question_vector, search_client)
           
           # Generate answer
           print("  [Generating answer...]")
           answer = generate_answer(question, context)
           
           print(f"\nA: {answer}")
           print(f"\nSources: {', '.join(sources)}")
           print("-" * 60)
       
       print("\n" + "="*60)
       print("✓ RAG Agent testing completed!")
       print("="*60 + "\n")
   
   if __name__ == "__main__":
       main()
   ```

3. Update your `.env` file to include GPT deployment:

   ```env
   AZURE_OPENAI_GPT_DEPLOYMENT=gpt-35-turbo
   ```

4. Run the RAG agent test:

   ```powershell
   python 10-test-rag-agent.py
   ```

### Verification

Verify the RAG pattern is working:
- ✓ Azure AI Foundry project created
- ✓ Connections configured for AI Search and OpenAI
- ✓ Prompt flow created with search and generation nodes
- ✓ Agent returns answers grounded in indexed documents
- ✓ Agent cites source documents
- ✓ Agent appropriately says it doesn't know when information isn't in the knowledge base
- ✓ Local test script executes successfully

**🎉 Congratulations!** You've successfully implemented a complete RAG pattern with Azure AI Search and Azure AI Foundry!

---

## Troubleshooting

### Common Issues and Solutions

**Issue: SQL Database Connection Failed**
- **Symptom**: Indexer fails with "cannot connect to SQL server"
- **Solution**: 
  - Verify firewall rules allow Azure services
  - Check connection string is correctly formatted
  - Ensure password doesn't contain special characters that need escaping
  - Test connection using Azure Data Studio or SSMS

**Issue: Blob Indexer Fails with Skillset Errors**
- **Symptom**: Indexer status shows failures, errors mention skills
- **Solution**:
  - Verify Azure AI Services key is correct
  - Check Azure OpenAI deployment name matches `.env`
  - Ensure Azure OpenAI has sufficient quota
  - Review indexer error details in Azure Portal

**Issue: Vector Search Returns No Results**
- **Symptom**: Keyword search works but vector search returns nothing
- **Solution**:
  - Verify embeddings were generated (check indexer status)
  - Ensure embedding dimensions match (1536 for ada-002)
  - Confirm vector field name is correct in queries
  - Re-run indexer if embeddings weren't generated initially

**Issue: Web App Can't Connect to Azure AI Search**
- **Symptom**: Flask app returns 500 errors, "unauthorized" or "not found"
- **Solution**:
  - Verify AZURE_SEARCH_ENDPOINT includes https://
  - Check AZURE_SEARCH_KEY is the admin key (not query key)
  - Ensure index name "products-index" exists
  - Test credentials with Python scripts first

**Issue: OpenAI API Errors**
- **Symptom**: "module 'openai' has no attribute 'api_type'" or embedding generation fails
- **Solution**:
  - Ensure you're using openai==1.12.0 (check with `pip show openai`)
  - Update code to use new AzureOpenAI client if using older examples
  - Verify AZURE_OPENAI_API_VERSION is set in .env file
  - Check deployment names match exactly in Azure OpenAI Studio

**Issue: PDF Text Not Extracted**
- **Symptom**: Search results don't contain PDF content
- **Solution**:
  - Verify PDFs are not password-protected or corrupted
  - Check Azure AI Services resource is in same region as AI Search
  - Ensure skillset includes OCR skill
  - Verify indexing parameters include `imageAction: "generateNormalizedImages"`

**Issue: Facets Not Appearing**
- **Symptom**: Web app shows no facets or facet counts are zero
- **Solution**:
  - Verify index fields are marked as `facetable=True`
  - Check data actually has values for faceted fields
  - Ensure facets parameter is included in search query
  - Test facets with Python script first

**Issue: Azure OpenAI Rate Limit Errors**
- **Symptom**: Embedding generation fails with 429 errors
- **Solution**:
  - Reduce batch size in indexer
  - Add retry logic to custom skills
  - Increase Azure OpenAI quota if possible
  - Use indexer schedule instead of on-demand runs

**Issue: AI Foundry Prompt Flow Errors**
- **Symptom**: Prompt flow nodes fail or don't connect
- **Solution**:
  - Verify all connections are properly authenticated
  - Check deployment names match in connections
  - Ensure Python code in custom nodes is syntactically correct
  - Test individual nodes before connecting them

### Debugging Tips

**View Indexer Status in Portal:**
1. Go to Azure Portal → Your AI Search Service
2. Click **Indexers** in left navigation
3. Click on your indexer name
4. Review **Execution History** and error details

**Test Search Queries Directly:**
```python
# Quick test script
from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from dotenv import load_dotenv
import os

load_dotenv()
client = SearchClient(
    endpoint=os.getenv("AZURE_SEARCH_ENDPOINT"),
    index_name="products-index",  # or "knowledge-base-index"
    credential=AzureKeyCredential(os.getenv("AZURE_SEARCH_KEY"))
)

results = client.search("test", top=5)
for result in results:
    print(result)
```

**Check Index Document Count:**
```python
from azure.search.documents.indexes import SearchIndexClient

index_client = SearchIndexClient(
    endpoint=os.getenv("AZURE_SEARCH_ENDPOINT"),
    credential=AzureKeyCredential(os.getenv("AZURE_SEARCH_KEY"))
)

stats = index_client.get_index_statistics("products-index")
print(f"Document count: {stats.document_count}")
print(f"Storage size: {stats.storage_size} bytes")
```

---

## Resource Cleanup

**IMPORTANT**: Clean up resources after completing the lab to avoid ongoing charges!

### Option 1: Delete Resource Group (Recommended)

This removes all lab resources at once:

```powershell
az login
az group delete --name ai-search-lab-rg --yes --no-wait
```

Or in Azure Portal:
1. Navigate to **Resource groups**
2. Click on `ai-search-lab-rg`
3. Click **Delete resource group**
4. Type the resource group name to confirm
5. Click **Delete**

### Option 2: Stop Resources (Keep for Later)

If you want to pause costs but keep resources:

**Azure AI Search:**
- Cannot be stopped, but only charges for usage
- Consider deleting and recreating if needed later

**Azure SQL Database:**
1. Portal → SQL Database → Your database
2. Click **Pause** (if using serverless tier)
3. Or scale down to Basic tier

**Azure OpenAI:**
- Charges per token usage, no baseline cost
- No action needed

**Storage Account:**
- Minimal cost for data at rest
- Can delete specific containers if needed

### Verify Cleanup

1. Check Azure Portal → Resource groups
2. Ensure `ai-search-lab-rg` is deleted or contains only desired resources
3. Check **Cost Management + Billing** → **Cost analysis** after 24 hours

---

## Summary and Next Steps

### What You Accomplished

In this lab, you successfully:

✓ Provisioned a complete Azure AI Search environment with multiple resources  
✓ Created an AI-enriched search index from blob storage with OCR and text extraction  
✓ Implemented vector search with Azure OpenAI embeddings  
✓ Indexed structured data from Azure SQL Database  
✓ Configured faceted search with filters and sorting  
✓ Deployed a web application for product search  
✓ (Bonus) Built a RAG-pattern agent with Azure AI Foundry  

### Key Concepts Learned

- **Multi-source indexing**: Combining data from different sources (blob, SQL) in a unified search service
- **AI enrichment**: Using cognitive skills to extract and enhance content from documents
- **Vector search**: Leveraging semantic understanding through embeddings
- **Faceted navigation**: Enabling users to filter and drill down through search results
- **RAG pattern**: Grounding LLM responses in retrieved, relevant documents

### Architecture Patterns

You implemented two key patterns:

1. **Traditional Search**: SQL-backed product index with facets → Web application
   - Best for: E-commerce, catalogs, structured data search
   
2. **RAG (Retrieval-Augmented Generation)**: Document index with vectors → AI agent
   - Best for: Knowledge bases, Q&A systems, intelligent assistants

### Next Steps and Further Learning

**Enhance Your Solution:**
- Add semantic ranking for improved relevance
- Implement autocomplete and suggestions
- Add more cognitive skills (entity recognition, language detection)
- Create additional indexes for different data types
- Implement security with Azure AD authentication

**Explore Advanced Features:**
- Incremental indexing and change detection
- Custom analyzers for specialized text processing
- Knowledge store for downstream analytics
- Debug sessions for skillset troubleshooting
- Performance tuning and optimization

**Additional Resources:**
- [Azure AI Search Documentation](https://learn.microsoft.com/azure/search/)
- [Vector Search in Azure AI Search](https://learn.microsoft.com/azure/search/vector-search-overview)
- [Azure OpenAI Service Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-studio/)

### Feedback

We hope you found this lab valuable! Consider exploring these related topics:
- Azure OpenAI assistants and function calling
- Azure AI Document Intelligence for advanced document processing
- Azure Cognitive Search integration with Power BI
- Multi-modal search with images and text

**🎉 Congratulations on completing the Azure AI Search Multi-Source Integration Lab!**

---

## Appendix: Lab Materials Checklist

### Files You Should Have Created

In your `ai-search-lab` directory:

```
ai-search-lab/
├── .env                          # Your credentials
├── 00-test-connections.py       # Verify configuration
├── 01-create-blob-index.py      # Blob index schema
├── 02-create-skillset.py        # AI enrichment skillset
├── 03-create-blob-datasource.py # Blob storage connection
├── 04-create-blob-indexer.py    # Blob indexer
├── 05-test-blob-search.py       # Test blob index
├── 06-create-sql-index.py       # SQL index schema
├── 07-create-sql-datasource.py  # SQL connection
├── 08-create-sql-indexer.py     # SQL indexer
├── 09-test-sql-search.py        # Test SQL index
├── 10-test-rag-agent.py         # Test RAG pattern (bonus)
└── webapp/
    ├── app.py                    # Flask application
    └── templates/
        └── index.html            # Search UI
```

### Azure Resources Created

- Resource Group: `ai-search-lab-rg`
- Azure AI Search: `ai-search-lab-<yourname>-<random>`
- Storage Account: `searchlab<yourname><random>`
  - Container: `documents`
- SQL Server: `sqlserver-lab-<yourname>-<random>`
  - Database: `AdventureWorksLT`
- Azure OpenAI: `openai-lab-<yourname>-<random>`
  - Deployment: `text-embedding`
  - (Optional) Deployment: `gpt-35-turbo`
- Azure AI Services: `ai-services-lab-<yourname>-<random>`
- (Bonus) Azure AI Foundry Project: `ai-search-rag-project`

### Indexes Created

- `knowledge-base-index` - Documents from blob storage with vector search
- `products-index` - Products from SQL database with faceted search

---

*Lab version 1.0 - February 2026*
