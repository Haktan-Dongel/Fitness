import { useState, useEffect } from 'react';
import {
  Box, Typography, Grid, Paper, CircularProgress, Alert,
  Table, TableBody, TableCell, TableRow, IconButton
} from '@mui/material';
import { statisticsAPI, dashboardAPI, MonthlyStats, ActivityOverview } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import FitnessCenterIcon from '@mui/icons-material/FitnessCenter';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import SpeedIcon from '@mui/icons-material/Speed';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';

interface StatsState {
  monthly: MonthlyStats | null;
  overview: ActivityOverview | null;
  yearlySummary: Record<number, Record<number, number>> | null;
  loading: boolean;
  error: string | null;
}

export default function Dashboard() {
  const { user, isAuthenticated } = useAuth();
  const [selectedDate, setSelectedDate] = useState(() => new Date());
  const [stats, setStats] = useState<StatsState>({
    monthly: null,
    overview: null,
    yearlySummary: null,
    loading: true,
    error: null
  });

  const navigateMonth = (direction: number) => {
    setSelectedDate(prev => {
      const newDate = new Date(prev);
      newDate.setMonth(prev.getMonth() + direction);
      return newDate;
    });
  };

  const navigateYear = (direction: number) => {
    setSelectedDate(prev => {
      const newDate = new Date(prev);
      newDate.setFullYear(prev.getFullYear() + direction);
      return newDate;
    });
  };

  useEffect(() => {
    const fetchStats = async () => {
      try {
        if (isAuthenticated && user) {
          const [monthlyResponse, overviewResponse, yearlySummaryResponse] = await Promise.all([
            statisticsAPI.getMonthlyStats(
              user.memberId,
              selectedDate.getFullYear(),
              selectedDate.getMonth() + 1
            ),
            statisticsAPI.getOverview(user.memberId),
            statisticsAPI.getYearlySummary(user.memberId)
          ]);

          setStats({
            monthly: monthlyResponse.data,
            overview: overviewResponse.data,
            yearlySummary: yearlySummaryResponse.data,
            loading: false,
            error: null
          });
        } else {
          // Get general stats for non-authenticated users
          const response = await dashboardAPI.getStats();
          setStats({
            monthly: null,
            overview: null,
            yearlySummary: null,
            loading: false,
            error: null,
            ...response.data
          });
        }
      } catch (error) {
        console.error('Error fetching stats:', error);
        setStats(prev => ({
          ...prev,
          loading: false,
          error: 'Failed to load statistics'
        }));
      }
    };

    fetchStats();
  }, [isAuthenticated, user, selectedDate]);

  if (stats.loading) return <CircularProgress />;
  if (stats.error) return <Alert severity="error">{stats.error}</Alert>;

  // Niet logged in weergave
  if (!isAuthenticated) {
    return (
      <Box>
        <Typography variant="h5" gutterBottom>Welcome to YouMove Fitness</Typography>
        <Typography variant="body1" gutterBottom color="text.secondary">
          Please log in to view your personal statistics.
        </Typography>
      </Box>
    );
  }

  const DateNavigation = () => (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
      <IconButton onClick={() => navigateYear(-1)}>
        <ArrowBackIcon />
        <ArrowBackIcon />
      </IconButton>
      <IconButton onClick={() => navigateMonth(-1)}>
        <ArrowBackIcon />
      </IconButton>
      <Typography sx={{ mx: 2 }}>
        {selectedDate.toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}
      </Typography>
      <IconButton onClick={() => navigateMonth(1)}>
        <ArrowForwardIcon />
      </IconButton>
      <IconButton onClick={() => navigateYear(1)}>
        <ArrowForwardIcon />
        <ArrowForwardIcon />
      </IconButton>
    </Box>
  );

  const YearlySummary = () => {
    if (!stats.yearlySummary) {
      console.debug("No yearly summary data available");
      return null;
    }

    const currentYear = selectedDate.getFullYear();
    const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

    console.debug('Rendering yearly summary for year:', currentYear);
    console.debug('Available data:', stats.yearlySummary);

    return (
      <Paper sx={{ p: 2, mt: 2 }}>
        <Typography variant="h6" gutterBottom>Activity History ({currentYear})</Typography>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', minHeight: 120 }}>
          {monthNames.map((month, idx) => {
            const monthNumber = idx + 1;
            const sessions = stats.yearlySummary?.[currentYear]?.[monthNumber] || 0;
            const isCurrentMonth = monthNumber === selectedDate.getMonth() + 1;
            
            return (
              <Box 
                key={month} 
                sx={{ 
                  display: 'flex', 
                  flexDirection: 'column', 
                  alignItems: 'center',
                  cursor: 'pointer',
                  opacity: isCurrentMonth ? 1 : 0.7,
                  '&:hover': {
                    opacity: 1
                  }
                }}
                onClick={() => {
                  const newDate = new Date(selectedDate);
                  newDate.setMonth(idx);
                  setSelectedDate(newDate);
                }}
              >
                <Typography variant="caption">{month}</Typography>
                <Box 
                  sx={{ 
                    width: 20, 
                    height: Math.max(Math.min(sessions * 10, 100), 5), 
                    backgroundColor: isCurrentMonth ? 'primary.main' : 'primary.light',
                    borderRadius: 1,
                    mt: 1,
                    transition: 'all 0.2s ease'
                  }} 
                />
                <Typography variant="caption" sx={{ mt: 0.5 }}>{sessions}</Typography>
              </Box>
            );
          })}
        </Box>
      </Paper>
    );
  };

  return (
    <Box>
      <Typography variant="h5" gutterBottom>Your Activity Dashboard</Typography>
      <DateNavigation />
      <YearlySummary />
      
      <Grid container spacing={3} sx={{ mt: 2 }}>
        {/* Monthly Stats */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              {selectedDate.toLocaleString('default', { month: 'long', year: 'numeric' })} Activity
            </Typography>
            <Table>
              <TableBody>
                <TableRow>
                  <TableCell>Total Sessions</TableCell>
                  <TableCell align="right">{stats.monthly?.totalSessions || 0}</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Total Hours</TableCell>
                  <TableCell align="right">{stats.monthly?.totalHours?.toFixed(1) || '0'}</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Average Duration</TableCell>
                  <TableCell align="right">
                    {stats.monthly?.averageDuration ? Math.round(stats.monthly.averageDuration) : 0} minutes
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>

            {stats.monthly?.sessionsPerType && Object.keys(stats.monthly.sessionsPerType).length > 0 && (
              <>
                <Typography variant="subtitle1" sx={{ mt: 2, mb: 1 }}>Sessions by Type</Typography>
                <Table size="small">
                  <TableBody>
                    {Object.entries(stats.monthly.sessionsPerType).map(([type, count]) => (
                      <TableRow key={type}>
                        <TableCell>{type}</TableCell>
                        <TableCell align="right">{count}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </>
            )}
          </Paper>
        </Grid>

        {/* Last Week Overview */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>Last 7 Days Overview</Typography>
            <Grid container spacing={2}>
              <Grid item xs={6} md={3}>
                <StatsCard
                  icon={<FitnessCenterIcon />}
                  title="Total Sessions"
                  value={stats.overview?.totalSessions || 0}
                />
              </Grid>
              <Grid item xs={6} md={3}>
                <StatsCard
                  icon={<AccessTimeIcon />}
                  title="Total Hours"
                  value={stats.overview?.totalDurationHours || 0}
                  unit="hours"
                />
              </Grid>
              <Grid item xs={6} md={3}>
                <StatsCard
                  icon={<SpeedIcon />}
                  title="Avg Duration"
                  value={Math.round(stats.overview?.averageDuration || 0)}
                  unit="min"
                />
              </Grid>
              <Grid item xs={6} md={3}>
                <StatsCard
                  icon={<CalendarMonthIcon />}
                  title="Longest"
                  value={stats.overview?.longestSession || 0}
                  unit="min"
                />
              </Grid>
            </Grid>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
}

interface StatsCardProps {
  icon: React.ReactNode;
  title: string;
  value: number;
  unit?: string;
}

function StatsCard({ icon, title, value, unit }: StatsCardProps) {
  return (
    <Box sx={{ textAlign: 'center' }}>
      <Box sx={{ color: 'primary.main', mb: 1 }}>{icon}</Box>
      <Typography variant="body2" color="text.secondary">{title}</Typography>
      <Typography variant="h6">
        {value}{unit ? ` ${unit}` : ''}
      </Typography>
    </Box>
  );
}
