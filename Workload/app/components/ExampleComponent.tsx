import React from 'react';
import { useGetWorkspaces, useFabricWorkspaceUsers } from '../hooks/queries/fabricQueries';
import { Layout } from '@carloconsultants/cs-ui-library';
import { Spinner, MessageBar, MessageBarType } from '@fluentui/react';

/**
 * Example component demonstrating the template patterns
 * - Uses TanStack Query for data fetching
 * - Uses cs-ui-library Layout component
 * - Implements proper loading and error states
 * - Shows Fabric workspace and user data
 */
export function ExampleComponent() {
  const { 
    data: workspaces, 
    isLoading: workspacesLoading, 
    error: workspacesError 
  } = useGetWorkspaces();

  const { 
    data: users, 
    isLoading: usersLoading, 
    error: usersError 
  } = useFabricWorkspaceUsers();

  if (workspacesLoading || usersLoading) {
    return (
      <Layout>
        <div style={{ padding: '20px', textAlign: 'center' }}>
          <Spinner label="Loading data..." />
        </div>
      </Layout>
    );
  }

  if (workspacesError || usersError) {
    return (
      <Layout>
        <div style={{ padding: '20px' }}>
          <MessageBar messageBarType={MessageBarType.error}>
            Error loading data: {workspacesError?.message || usersError?.message}
          </MessageBar>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div style={{ padding: '20px' }}>
        <h1>Fabric Workload Template Example</h1>
        
        <div style={{ marginBottom: '20px' }}>
          <h2>Workspaces ({workspaces?.length || 0})</h2>
          {workspaces?.map((workspace) => (
            <div key={workspace.id} style={{ 
              padding: '10px', 
              margin: '5px 0', 
              border: '1px solid #ccc',
              borderRadius: '4px'
            }}>
              <strong>{workspace.displayName}</strong>
              <br />
              <small>ID: {workspace.id}</small>
            </div>
          ))}
        </div>

        <div>
          <h2>Users ({users?.length || 0})</h2>
          {users?.map((user) => (
            <div key={user.id} style={{ 
              padding: '10px', 
              margin: '5px 0', 
              border: '1px solid #ccc',
              borderRadius: '4px'
            }}>
              <strong>{user.displayName || 'Unknown'}</strong>
              <br />
              <small>Email: {user.email || 'N/A'}</small>
              <br />
              <small>Role: {user.workspaceRole || 'N/A'}</small>
            </div>
          ))}
        </div>
      </div>
    </Layout>
  );
}
