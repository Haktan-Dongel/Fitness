import { Outlet, useLocation } from 'react-router-dom';
import { Box, AppBar, Toolbar, Typography, Drawer, useTheme, IconButton, Button } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import DashboardIcon from '@mui/icons-material/Dashboard';
import PeopleIcon from '@mui/icons-material/People';
import EventIcon from '@mui/icons-material/Event';
import MenuIcon from '@mui/icons-material/Menu';
import PersonIcon from '@mui/icons-material/AccountCircle';
import { useState, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import LoginDialog from './LoginDialog';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import LogoutIcon from '@mui/icons-material/Logout';

interface MenuItem {
  text: string;
  icon: JSX.Element;
  path: string;
}

const DRAWER_WIDTH = 240;

const PAGE_TITLES: Record<string, string> = {
  '/': 'Dashboard',
  '/members': 'Members',
  '/reservations': 'Reservations',
};

const NavigationDrawer = ({ 
  mobileOpen, 
  handleDrawerToggle, 
  currentPath,
  onNavigate 
}: {
  mobileOpen: boolean;
  handleDrawerToggle: () => void;
  currentPath: string;
  onNavigate: (path: string) => void;
}) => {
  const drawerContent = useMemo(() => (
    <DrawerContent currentPath={currentPath} onNavigate={onNavigate} />
  ), [currentPath, onNavigate]);

  return (
    <Box component="nav" sx={{ width: { md: DRAWER_WIDTH }, flexShrink: { md: 0 } }}>
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={handleDrawerToggle}
        ModalProps={{ keepMounted: true }}
        sx={{
          display: { xs: 'block', md: 'none' },
          '& .MuiDrawer-paper': { width: DRAWER_WIDTH, boxSizing: 'border-box' },
        }}
      >
        {drawerContent}
      </Drawer>
      
      <Drawer
        variant="permanent"
        sx={{
          display: { xs: 'none', md: 'block' },
          '& .MuiDrawer-paper': { width: DRAWER_WIDTH, boxSizing: 'border-box' },
        }}
        open
      >
        {drawerContent}
      </Drawer>
    </Box>
  );
};

const DrawerContent = ({ currentPath, onNavigate }: { currentPath: string; onNavigate: (path: string) => void }) => {
  const theme = useTheme();
  const { isAuthenticated } = useAuth();

  const MENU_ITEMS: MenuItem[] = [
    { text: 'Dashboard', icon: <DashboardIcon />, path: '/' },
    ...(!isAuthenticated ? [
      { text: 'Members', icon: <PeopleIcon />, path: '/members' }
    ] : []),
    ...(isAuthenticated ? [
      { text: 'Profile', icon: <PersonIcon />, path: '/profile' },
      { text: 'Reservations', icon: <EventIcon />, path: '/reservations' }
    ] : [])
  ];

  return (
    <Box sx={{ mt: 8, overflow: 'auto' }}>
      {MENU_ITEMS.map((item) => (
        <Box
          component="button"
          key={item.text}
          onClick={() => onNavigate(item.path)}
          sx={{
            width: '100%',
            padding: 2,
            display: 'flex',
            alignItems: 'center',
            border: 'none',
            backgroundColor: currentPath === item.path ? 'rgba(0, 0, 0, 0.04)' : 'transparent',
            color: currentPath === item.path ? theme.palette.primary.main : 'inherit',
            cursor: 'pointer',
            '&:hover': {
              backgroundColor: 'rgba(0, 0, 0, 0.08)',
            },
          }}
        >
          <Box sx={{ mr: 2, color: 'inherit' }}>{item.icon}</Box>
          <Typography>{item.text}</Typography>
        </Box>
      ))}
    </Box>
  );
};

const Breadcrumbs = ({ pathnames }: { pathnames: string[] }) => {
  const theme = useTheme();

  if (pathnames.length === 0) {
    return <Typography color="text.primary">Dashboard</Typography>;
  }

  return pathnames.map((value, index) => {
    const last = index === pathnames.length - 1;
    const to = `/${pathnames.slice(0, index + 1).join('/')}`;

    return last ? (
      <Typography color="text.primary" key={to}>
        {PAGE_TITLES[to] || value}
      </Typography>
    ) : (
      <Link
        key={to}
        to={to}
        style={{ 
          color: theme.palette.primary.main,
          textDecoration: 'none'
        }}
      >
        {PAGE_TITLES[to] || value}
      </Link>
    );
  });
};

export default function Layout() {
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const [loginDialogOpen, setLoginDialogOpen] = useState(false);
  const { user, logout, isAuthenticated } = useAuth();
  const pathnames = useMemo(() => 
    location.pathname.split('/').filter(Boolean),
    [location.pathname]
  );

  const handleNavigate = (path: string) => {
    navigate(path);
    setMobileOpen(false);
  };

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <AppBar position="fixed">
        <Toolbar sx={{ justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Box
              component="button"
              onClick={() => setMobileOpen(!mobileOpen)}
              sx={{
                mr: 2,
                display: { md: 'none' },
                border: 'none',
                background: 'none',
                color: 'inherit',
                cursor: 'pointer',
              }}
              aria-label="Toggle menu"
            >
              <MenuIcon />
            </Box>
            <Typography variant="h6" component="h1" sx={{ color: 'primary.main' }}>
              Fitness
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            {isAuthenticated ? (
              <>
                <Typography>
                  {user?.firstName} {user?.lastName} ({user?.memberType})
                </Typography>
                <IconButton 
                  color="inherit" 
                  onClick={logout}
                  size="small"
                >
                  <LogoutIcon />
                </IconButton>
              </>
            ) : (
              <Button
                variant="outlined"
                color="inherit"
                startIcon={<AccountCircleIcon />}
                onClick={() => setLoginDialogOpen(true)}
              >
                Member Login
              </Button>
            )}
          </Box>
        </Toolbar>
      </AppBar>

      <NavigationDrawer
        mobileOpen={mobileOpen}
        handleDrawerToggle={() => setMobileOpen(!mobileOpen)}
        currentPath={location.pathname}
        onNavigate={handleNavigate}
      />

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          backgroundColor: 'background.default',
          mt: 8,
        }}
      >
        <Typography variant="h5" component="h2" sx={{ mb: 1 }}>
          {PAGE_TITLES[location.pathname] || 'Dashboard'}
        </Typography>
        <Box sx={{ mb: 3 }}>
          <Breadcrumbs pathnames={pathnames} />
        </Box>
        <Outlet />
      </Box>

      <LoginDialog
        open={loginDialogOpen}
        onClose={() => setLoginDialogOpen(false)}
      />
    </Box>
  );
}
