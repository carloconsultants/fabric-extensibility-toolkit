import { useQuery } from '@tanstack/react-query'
import { FabricPlatformAPIClient, WorkloadClientAPI } from '../../clients'

// Singleton Fabric Client Instance
let fabricClient: FabricPlatformAPIClient | null = null

export function initializeFabricClient(workloadClient: WorkloadClientAPI): FabricPlatformAPIClient {
  console.log('Initializing Fabric Client')
  if (!fabricClient) {
    fabricClient = FabricPlatformAPIClient.create(workloadClient)
  }
  return fabricClient
}

export function getFabricClient(): FabricPlatformAPIClient {
  if (!fabricClient) {
    throw new Error('Fabric client not initialized. Call initializeFabricClient first.')
  }
  return fabricClient
}

// Export the client instance for use in other modules
export { fabricClient }

// Query Definitions:

export const getWorkspaces = 'get-workspaces'
export function useGetWorkspaces() {
  return useQuery({
    queryKey: [getWorkspaces],
    queryFn: async () => {
      console.log('🔍 Starting workspace fetch...')
      try {
        const result = await fabricClient?.workspaces.getAllWorkspaces()
        console.log('✅ Workspaces fetched successfully:', result)
        return result
      } catch (error) {
        console.error('❌ Error fetching workspaces:', error)
        throw error
      }
    },
  })
}

export const getLakehousesKey = 'get-lakehouses'
export function useGetLakehouses(workspaceId: string) {
  return useQuery([getLakehousesKey, workspaceId], async () => {
    return fabricClient?.lakehouses.getAllLakehouses(workspaceId)
  })
}

export const getLakehouseTables = 'get-lakehouse-tables'
export function useGetLakehouseTables(
  workspaceId: string,
  lakehouseId: string,
  options?: { enabled?: boolean }
) {
  return useQuery(
    ['lakehouse-tables', workspaceId, lakehouseId],
    async () => {
      return fabricClient?.lakehouses.getAllTables(workspaceId, lakehouseId)
    },
    {
      enabled: options?.enabled ?? true,
      ...options,
    }
  )
}
