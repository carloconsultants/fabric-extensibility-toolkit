import { useQuery } from '@tanstack/react-query'
import { WorkspaceRoleAssignment, WorkspaceRole } from '../../clients/FabricPlatformTypes'
import { getFabricClient } from './fabricQueries'

/**
 * Fabric-based user queries that don't require Graph API permissions
 * Uses Fabric Platform workspace role assignments to get user information
 */

export interface FabricUser {
  id: string
  displayName?: string
  email?: string
  type: 'User' | 'Group' | 'ServicePrincipal' | 'ManagedIdentity'
  workspaceRole?: WorkspaceRole
  workspaceId?: string
}

/**
 * Get users from all accessible workspaces using Fabric API
 * This replaces the Graph API-based getUsersByTenant call
 */
export function useFabricWorkspaceUsers() {
  return useQuery({
    queryKey: ['fabric-workspace-users'],
    queryFn: async (): Promise<FabricUser[]> => {
      const fabricClient = getFabricClient()

      try {
        // Get all workspaces the user has access to
        const workspaces = await fabricClient.workspaces.getAllWorkspaces()

        // Collect unique users from all workspace role assignments
        const allUsers = new Map<string, FabricUser>()

        for (const workspace of workspaces) {
          try {
            // Get role assignments for this workspace
            const roleAssignments = await fabricClient.workspaces.getAllWorkspaceRoleAssignments(
              workspace.id
            )

            for (const assignment of roleAssignments) {
              const principal = assignment.principal

              // Only include User principals (not Groups, ServicePrincipals, etc.)
              if (principal.type === 'User') {
                const existingUser = allUsers.get(principal.id)

                const user: FabricUser = {
                  id: principal.id,
                  displayName: principal.profile?.displayName || existingUser?.displayName,
                  email: principal.profile?.email || existingUser?.email,
                  type: principal.type,
                  workspaceRole: assignment.role,
                  workspaceId: workspace.id,
                }

                // If user already exists, keep the highest role
                if (existingUser && existingUser.workspaceRole) {
                  const roleHierarchy: Record<WorkspaceRole, number> = {
                    Viewer: 1,
                    Contributor: 2,
                    Member: 3,
                    Admin: 4,
                  }
                  const currentRoleLevel = roleHierarchy[assignment.role as WorkspaceRole]
                  const existingRoleLevel =
                    roleHierarchy[existingUser.workspaceRole as WorkspaceRole]

                  if (currentRoleLevel > existingRoleLevel) {
                    user.workspaceRole = assignment.role
                  } else {
                    user.workspaceRole = existingUser.workspaceRole
                  }
                }

                allUsers.set(principal.id, user)
              }
            }
          } catch (error) {
            console.warn(`Failed to get role assignments for workspace ${workspace.id}:`, error)
            // Continue with other workspaces
          }
        }

        return Array.from(allUsers.values())
      } catch (error) {
        console.error('Error fetching Fabric workspace users:', error)
        throw error
      }
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
  })
}

/**
 * Get users for a specific workspace using Fabric API
 */
export function useFabricWorkspaceUsersByWorkspace(workspaceId: string) {
  return useQuery({
    queryKey: ['fabric-workspace-users', workspaceId],
    queryFn: async (): Promise<FabricUser[]> => {
      const fabricClient = getFabricClient()

      try {
        const roleAssignments =
          await fabricClient.workspaces.getAllWorkspaceRoleAssignments(workspaceId)

        return roleAssignments
          .filter((assignment: WorkspaceRoleAssignment) => assignment.principal.type === 'User')
          .map((assignment: WorkspaceRoleAssignment) => ({
            id: assignment.principal.id,
            displayName: assignment.principal.profile?.displayName,
            email: assignment.principal.profile?.email,
            type: assignment.principal.type as 'User',
            workspaceRole: assignment.role,
            workspaceId: workspaceId,
          }))
      } catch (error) {
        console.error(`Error fetching users for workspace ${workspaceId}:`, error)
        throw error
      }
    },
    enabled: !!workspaceId,
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
  })
}
