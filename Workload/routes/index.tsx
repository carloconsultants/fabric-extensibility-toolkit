import { createFileRoute } from '@tanstack/react-router';
import React, { useState } from 'react';
import { Layout } from '../lib/cs-ui-library/src/components/Layout';
import { NavSection } from '../lib/cs-ui-library/src/components/NavDrawer';
import { ToolbarTab } from '../lib/cs-ui-library/src/components/AppToolbar';
import { Button, Title2, Body1, Card } from '@fluentui/react-components';
import { 
  Home24Regular, 
  Settings24Regular, 
  Info24Regular,
  Add24Regular,
  Delete24Regular,
  Edit24Regular,
  Save24Regular,
  Share24Regular
} from '@fluentui/react-icons';

export const Route = createFileRoute('/')({
  component: LandingPage,
});

function LandingPage() {
  const { workloadClient } = Route.useRouteContext();
  const [selectedNav, setSelectedNav] = useState('home');

  const navSections: NavSection[] = [
    {
      items: [
        {
          value: 'home',
          title: 'Home',
          icon: Home24Regular,
          navigate: () => setSelectedNav('home'),
        },
        {
          value: 'about',
          title: 'About',
          icon: Info24Regular,
          navigate: () => setSelectedNav('about'),
        },
        {
          value: 'settings',
          title: 'Settings',
          icon: Settings24Regular,
          navigate: () => setSelectedNav('settings'),
        },
      ],
    },
  ];

  const toolbarTabs: ToolbarTab[] = [
    {
      title: 'Overview',
      leftItems: [
        {
          icon: <Add24Regular />,
          label: 'New',
          onClick: () => alert('Create new item'),
          appearance: 'subtle',
        },
        {
          icon: <Edit24Regular />,
          label: 'Edit',
          onClick: () => alert('Edit item'),
        },
        {
          icon: <Delete24Regular />,
          label: 'Delete',
          onClick: () => alert('Delete item'),
          divider: true,
        },
        {
          icon: <Save24Regular />,
          onClick: () => alert('Save changes'),
        },
        {
          icon: <Share24Regular />,
          label: 'Share',
          onClick: () => alert('Share content'),
        },
      ],
    },
    {
      title: 'Details',
      leftItems: [
        {
          icon: <Save24Regular />,
          onClick: () => alert('Save details'),
        },
        {
          icon: <Edit24Regular />,
          label: 'Edit Details',
          onClick: () => alert('Edit details'),
          appearance: 'subtle',
        },
      ],
    },
  ];

  const renderContent = () => {
    if (selectedNav === 'home') {
      return (
        <div style={{ padding: '24px' }}>
          <Title2>Welcome to Your Landing Page</Title2>
          <Body1 style={{ marginTop: '16px' }}>
            This is a simple landing page built using the cs-ui-library Layout component
            with TanStack Router for file-based routing.
          </Body1>
          <div style={{ marginTop: '24px', display: 'flex', gap: '16px' }}>
            <Card style={{ padding: '16px', flex: 1 }}>
              <Title2 style={{ fontSize: '18px' }}>Getting Started</Title2>
              <Body1 style={{ marginTop: '8px' }}>
                Explore the navigation on the left to discover different sections.
              </Body1>
            </Card>
            <Card style={{ padding: '16px', flex: 1 }}>
              <Title2 style={{ fontSize: '18px' }}>Features</Title2>
              <Body1 style={{ marginTop: '8px' }}>
                Use the tabs above to switch between different views of your content.
              </Body1>
            </Card>
          </div>
        </div>
      );
    } else if (selectedNav === 'about') {
      return (
        <div style={{ padding: '24px' }}>
          <Title2>About</Title2>
          <Body1 style={{ marginTop: '16px' }}>
            This landing page demonstrates TanStack Router with cs-ui-library Layout.
          </Body1>
        </div>
      );
    } else if (selectedNav === 'settings') {
      return (
        <div style={{ padding: '24px' }}>
          <Title2>Settings</Title2>
          <Body1 style={{ marginTop: '16px' }}>
            Configure your preferences here.
          </Body1>
        </div>
      );
    }
    return null;
  };

  return (
    <Layout
      navSections={navSections}
      toolbarTabs={toolbarTabs}
      selectedNavValue={selectedNav}
      defaultOpen={true}
      navType="inline"
      contentHovered={true}
      title="My Landing Page"
    >
      {renderContent()}
    </Layout>
  );
}
