import { useState, useEffect } from 'react';
import { 
  Box, Typography, Paper, CircularProgress, Alert, TextField,
  Button
} from '@mui/material';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import DeleteIcon from '@mui/icons-material/Delete';
import { reservationAPI, timeSlotAPI, equipmentAPI, memberAPI, CreateReservationDto } from '../services/api';
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

interface Reservation {
  reservationId: number;
  memberId: number;
  equipmentId: number;
  timeSlotId: number;
  date: string;
}

interface Member {
  memberId: number;
  firstName: string;
  lastName: string;
}

interface Equipment {
  equipmentId: number;
  deviceType: string;
}

interface TimeSlot {
  timeSlotId: number;
  startTime: string | Date;
  endTime: string | Date;
  partOfDay: string;
}

export default function Reservations() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const { user } = useAuth();
  const [enhancedReservations, setEnhancedReservations] = useState<EnhancedReservation[]>([]);

  const formatTime = (time: string | number | Date | null | undefined): string => {
    if (time === null || time === undefined) return 'N/A';
    
    if (typeof time === 'number') {
      return `${time.toString().padStart(2, '0')}:00`;
    }
    
    try {
      if (typeof time === 'string') {
        const match = time.match(/(\d{2}):(\d{2})/);
        if (match) {
          return `${match[1]}:${match[2]}`;
        }
        const date = new Date(time);
        if (!isNaN(date.getTime())) {
          return date.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
          });
        }
      }
      if (time instanceof Date) {
        return time.toLocaleTimeString('en-US', {
          hour: '2-digit',
          minute: '2-digit',
          hour12: false
        });
      }
      console.warn('Unhandled time format:', time);
      return 'Invalid Time';
    } catch (error) {
      console.error('Error formatting time:', error, time);
      return 'Invalid Time';
    }
  };

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
          onClick={() => handleDelete(params.row.reservationId)}
        >
          Delete
        </Button>
      ),
    }
  ];

  const processReservation = (reservation: Reservation, members: Member[], equipment: Equipment[], timeSlots: TimeSlot[]) => {
    try {
      const member = members.find(m => m.memberId === reservation.memberId);
      const equipmentItem = equipment.find(e => e.equipmentId === reservation.equipmentId);
      const timeSlot = timeSlots.find(t => t.timeSlotId === reservation.timeSlotId);
      
      const reservationDate = new Date(reservation.date);
      const today = new Date();
      today.setHours(0, 0, 0, 0);

      const enhancedReservation = {
        reservationId: reservation.reservationId,
        memberName: member 
          ? `${member.firstName} ${member.lastName}` 
          : `Unknown (ID: ${reservation.memberId})`,
        equipmentName: equipmentItem?.deviceType || `Unknown Equipment (ID: ${reservation.equipmentId})`,
        timeSlotName: timeSlot 
          ? `${timeSlot.partOfDay} (${formatTime(timeSlot.startTime)} - ${formatTime(timeSlot.endTime)})` 
          : `Unknown Time Slot (ID: ${reservation.timeSlotId})`,
        date: reservation.date,
        isPast: reservationDate < today
      };

      console.log('Enhanced reservation:', enhancedReservation);
      return enhancedReservation;
    } catch (error) {
      console.error('Error processing reservation:', error, reservation);
      return {
        reservationId: reservation.reservationId,
        memberName: `Unknown (ID: ${reservation.memberId})`,
        equipmentName: `Unknown Equipment (ID: ${reservation.equipmentId})`,
        timeSlotName: `Unknown Time Slot (ID: ${reservation.timeSlotId})`,
        date: reservation.date,
        isPast: false
      };
    }
  };

  const fetchReservations = async () => {
    try {
      setLoading(true);
      const [reservationsResponse, timeSlotsResponse, equipmentResponse, membersResponse] = await Promise.all([
        reservationAPI.getAll(),
        timeSlotAPI.getAll(),
        equipmentAPI.getAll(),
        memberAPI.getAll()
      ]);

      if (reservationsResponse.data.length > 0) {
        console.log('Example reservation date:', {
          rawDate: reservationsResponse.data[0].date,
          parsed: new Date(reservationsResponse.data[0].date),
          formatted: reservationsResponse.data[0].date?.split('T')[0] || 'Invalid Date'
        });
      }

      console.log('Raw data:', {
        timeSlots: timeSlotsResponse.data,
        equipment: equipmentResponse.data,
        reservations: reservationsResponse.data,
        dateExample: reservationsResponse.data[0]?.date
      });

      const enhanced = reservationsResponse.data.map(reservation => 
        processReservation(
          reservation, 
          membersResponse.data, 
          equipmentResponse.data, 
          timeSlotsResponse.data
        )
      );

      const filteredReservations = user 
        ? enhanced.filter(r => r.memberName.includes(`${user.firstName} ${user.lastName}`))
        : enhanced;

      setEnhancedReservations(filteredReservations);
      setError(null);
    } catch (error) {
      console.error('Error fetching reservations:', error);
      setError('Failed to load reservations');
      setEnhancedReservations([]);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await reservationAPI.delete(id);
      await fetchReservations();
    } catch (error) {
      console.error('Error deleting reservation:', error);
      setError('Failed to delete reservation');
    }
  };

  useEffect(() => {
    fetchReservations();
  }, [selectedDate]);

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h5">Reservations</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <TextField
            type="date"
            value={selectedDate}
            onChange={(e) => setSelectedDate(e.target.value)}
            InputLabelProps={{ shrink: true }}
            sx={{ width: 200 }}
          />
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
        onSave={async (data: CreateReservationDto) => {
          try {
            if (!user) {
              throw new Error('Must be logged in to create reservations');
            }

            const formattedData = {
              memberId: user.memberId,
              equipmentId: Number(data.equipmentId),
              timeSlotId: Number(data.timeSlotId),
              date: data.date
            };

            console.log('Creating reservation:', formattedData);
            
            await reservationAPI.create(formattedData);
            await fetchReservations();
            setCreateDialogOpen(false);
            setError(null);
          } catch (error: unknown) {
            const errorMessage = error instanceof Error ? error.message : 'Failed to create reservation';
            setError(errorMessage);
            console.error('Creation failed:', errorMessage);
          }
        }}
      />     {error && (        <Alert           severity="error"           sx={{ mt: 2 }}          onClose={() => setError(null)}        >          {error}        </Alert>      )}    </Box>  );}