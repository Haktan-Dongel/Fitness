import { useState, useEffect } from 'react';
import { 
  Box, Typography, Paper, CircularProgress, Alert, Button
} from '@mui/material';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import DeleteIcon from '@mui/icons-material/Delete';
import { reservationAPI, memberAPI, CreateReservationDto } from '../services/api';
import CreateReservationDialog from '../components/CreateReservationDialog';
import AddIcon from '@mui/icons-material/Add';
import { useAuth } from '../contexts/AuthContext';

interface EnhancedReservation {
  reservationId: number;
  memberName: string;
  equipmentName: string;
  timeSlotName: string;
  date: string;
  isPast: boolean;
}

export default function Reservations() {
  const [loading, setLoading] = useState(true);
  const [deleteInProgress, setDeleteInProgress] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const { user } = useAuth();
  const [enhancedReservations, setEnhancedReservations] = useState<EnhancedReservation[]>([]);

  const handleDelete = async (id: number) => {
    try {
      setDeleteInProgress(true);
      setError(null);
      
      if (!id) {
        throw new Error('Invalid reservation ID');
      }

      await reservationAPI.delete(id);
      await fetchReservations();
      
    } catch (error) {
      console.error('Error deleting reservation:', error);
      setError('Failed to delete reservation. Please try again.');
    } finally {
      setDeleteInProgress(false);
    }
  };

  const fetchReservations = async () => {
    try {
      setLoading(true);
      setError(null);
      
      if (!user) {
        setError('Must be logged in to view reservations');
        return;
      }

      const response = await memberAPI.getReservations(user.memberId);
      
      const enhanced = response.data.map(reservation => ({
        reservationId: reservation.reservationId,
        memberName: user ? `${user.firstName} ${user.lastName}` : 'Unknown',
        equipmentName: reservation.equipment,
        timeSlotName: reservation.timeSlot,
        date: reservation.date,
        isPast: new Date(reservation.date) < new Date()
      }));

      setEnhancedReservations(enhanced);
    } catch (error) {
      console.error('Error fetching reservations:', error);
      setError('Failed to load reservations');
      setEnhancedReservations([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!deleteInProgress) {
      fetchReservations();
    }
  }, [selectedDate, deleteInProgress, user]);

  const columns: GridColDef[] = [
    { field: 'reservationId', headerName: 'ID', width: 90 },
    { field: 'memberName', headerName: 'Member', width: 200 },
    { 
      field: 'date', 
      headerName: 'Date', 
      width: 180,
      renderCell: (params) => {
        return params.row.date?.split('T')[0] || 'Invalid Date';
      }
    },
    { field: 'timeSlotName', headerName: 'Time Slot', width: 150 },
    { field: 'equipmentName', headerName: 'Equipment', width: 150 },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 120,
      renderCell: (params) => (
        <Button
          startIcon={<DeleteIcon />}
          color="error"
          size="small"
          onClick={(e) => {
            e.stopPropagation();
            e.preventDefault();
            handleDelete(params.row.reservationId);
          }}
          disabled={deleteInProgress || params.row.isPast}
        >
          {deleteInProgress ? 'Deleting...' : 'Delete'}
        </Button>
      ),
    }
  ];

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h5">Reservations</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setCreateDialogOpen(true)}
          >
            New Reservation
          </Button>
        </Box>
      </Box>

      {/* Upcoming Reservations */}
      <Paper sx={{ mb: 3 }}>
        <Typography variant="h6" sx={{ p: 2, backgroundColor: 'primary.main', color: 'white' }}>
          Upcoming Reservations ({enhancedReservations.filter(r => !r.isPast).length})
        </Typography>
        <Box sx={{ height: 300, width: '100%' }}>
          <DataGrid
            rows={enhancedReservations.filter(r => !r.isPast)}
            columns={columns}
            getRowId={(row) => row.reservationId}
            initialState={{
              pagination: { paginationModel: { pageSize: 5 } },
              sorting: { sortModel: [{ field: 'date', sort: 'asc' }] },
            }}
            pageSizeOptions={[5]}
            disableRowSelectionOnClick
          />
        </Box>
      </Paper>

      {/* Past Reservations */}
      <Paper>
        <Typography variant="h6" sx={{ p: 2, backgroundColor: 'grey.300' }}>
          Past Reservations ({enhancedReservations.filter(r => r.isPast).length})
        </Typography>
        <Box sx={{ height: 300, width: '100%' }}>
          <DataGrid
            rows={enhancedReservations.filter(r => r.isPast)}
            columns={columns.filter(col => col.field !== 'actions')}
            getRowId={(row) => row.reservationId}
            initialState={{
              pagination: { paginationModel: { pageSize: 5 } },
              sorting: { sortModel: [{ field: 'date', sort: 'desc' }] },
            }}
            pageSizeOptions={[5]}
            disableRowSelectionOnClick
          />
        </Box>
      </Paper>

      <CreateReservationDialog
        open={createDialogOpen}
        onClose={() => {
          setCreateDialogOpen(false);
          setError(null);
        }}
        onSave={async (formData) => {
          try {
            if (!user) {
              throw new Error('Must be logged in to create reservations');
            }

            const reservationData: CreateReservationDto = {
              memberId: user.memberId,
              equipmentId: Number(formData.equipmentId),
              timeSlotIds: formData.timeSlotIds,
              date: formData.date,
            };

            console.log('Creating reservation:', reservationData);
            
            const response = await reservationAPI.create(reservationData);
            console.log('Reservation created:', response.data);
            
            await fetchReservations();
            setCreateDialogOpen(false);
            setError(null);
          } catch (error: unknown) {
            const errorMessage = error instanceof Error ? error.message : 'Failed to create reservation';
            setError(errorMessage);
            console.error('Creation failed:', errorMessage);
          }
        }}
      />
      {error && (
        <Alert
          severity="error"
          sx={{ mt: 2 }}
          onClose={() => setError(null)}
        >
          {error}
        </Alert>
      )}
    </Box>
  );
}