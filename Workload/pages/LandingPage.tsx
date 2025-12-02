import React, { useState } from 'react';
import { Layout } from '../lib/cs-ui-library/src/components/Layout';
import { NavSection } from '../lib/cs-ui-library/src/components/NavDrawer';
import { ToolbarTab } from '../lib/cs-ui-library/src/components/AppToolbar';
import { Button, Title2, Body1, Card } from '@fluentui/react-components';
import { Home24Regular, Settings24Regular, Info24Regular } from '@fluentui/react-icons';
import { PageProps } from '../App';

export function LandingPage({ workloadClient }: PageProps) {
  const [selectedNav, setSelectedNav] = useState('home');
  const [selectedTab, setSelectedTab] = useState('overview');

  // Define navigation sections
  const navSections: NavSection[] = [
    {
      items: [
        {
          value: 'home',
          label: 'Home',
          icon: <Home24Regular />,
          onClick: () => setSelectedNav('home'),
        },
        {
          value: 'about',
          label: 'About',
          icon: <Info24Regular />,
          onClick: () => setSelectedNav('about'),
        },
        {
          value: 'settings',
          label: 'Settings',
          icon: <Settings24Regular />,
          onClick: () => setSelectedNav('settings'),
        },
      ],
    },
  ];

  // Define toolbar tabs
  const toolbarTabs: ToolbarTab[] = [
    {
      value: 'overview',
      label: 'Overview',
      onClick: () => setSelectedTab('overview'),
    },
    {
      value: 'details',
      label: 'Details',
      onClick: () => setSelectedTab('details'),
    },
  ];

  // Render content based on selected navigation and tab
  const renderContent = () => {
    if (selectedNav === 'home') {
      if (selectedTab === 'overview') {
        return (
          <div style={{ padding: '24px' }}>
            <Title2>Welcome to Your Landing Page</Title2>
            <Body1 style={{ marginTop: '16px' }}>
              This is a simple landing page built using the cs-ui-library Layout component.
              The layout provides a consistent navigation structure with a sidebar and toolbar.
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
      } else if (selectedTab === 'details') {
        return (
          <div style={{ padding: '24px' }}>
            <Title2>Home Details</Title2>
            <Body1 style={{ marginTop: '16px' }}>
              This is the details view for the home section. You can add more
              specific information or data visualizations here.
            </Body1>
          </div>
        );
      }
    } else if (selectedNav === 'about') {
      return (
        <div style={{ padding: '24px' }}>
          <Title2>About</Title2>
          <Body1 style={{ marginTop: '16px' }}>
            This landing page demonstrates the cs-ui-library Layout component
            integrated into the Fabric Extensibility Toolkit. The Layout provides:
          </Body1>
          <ul style={{ marginTop: '16px', marginLeft: '24px' }}>
            <li>Sidebar navigation with customizable sections</li>
            <li>Top toolbar with tabs and action buttons</li>
            <li>Responsive design that works in Fabric workloads</li>
            <li>Consistent styling with Fluent UI components</li>
          </ul>
        </div>
      );
    } else if (selectedNav === 'settings') {
      return (
        <div style={{ padding: '24px' }}>
          <Title2>Settings</Title2>
          <Body1 style={{ marginTop: '16px' }}>
            Configure your preferences and settings here.
          </Body1>
          <div style={{ marginTop: '24px' }}>
            <Button appearance="primary">Save Settings</Button>
          </div>
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
