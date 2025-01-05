import { useState, useEffect } from 'react';
import { 
  Box, Typography, Paper, CircularProgress, Alert, Button,
  Dialog, DialogTitle, DialogContent, DialogActions, DialogContentText
} from '@mui/material';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import AddIcon from '@mui/icons-material/Add';
import { Member, memberAPI, MemberReservationView } from '../services/api';
import ReservationDialog from '../components/ReservationDialog';
import MemberDialog from '../components/MemberDialog';

export default function Members() {
  const [members, setMembers] = useState<Member[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editMember, setEditMember] = useState<Member | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [memberToDelete, setMemberToDelete] = useState<Member | null>(null);
  const [reservations, setReservations] = useState<MemberReservationView[]>([]);
  const [reservationDialogOpen, setReservationDialogOpen] = useState(false);
  const [selectedMember, setSelectedMember] = useState<Member | null>(null);

  const columns: GridColDef[] = [
    { field: 'memberId', headerName: 'ID', width: 70 },
    { field: 'firstName', headerName: 'First Name', width: 130 },
    { field: 'lastName', headerName: 'Last Name', width: 130 },
    { field: 'email', headerName: 'Email', width: 200 },
    { field: 'address', headerName: 'Address', width: 200 },
    { 
      field: 'birthday', 
      headerName: 'Birthday', 
      width: 120,
      renderCell: (params) => {
        return params.row.birthday?.split('T')[0] || 'Invalid Date';
      }
    },
    { field: 'memberType', headerName: 'Type', width: 100 },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 320,
      sortable: false,
      renderCell: (params) => (
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            size="small"
            startIcon={<EditIcon />}
            onClick={() => {
              setEditMember(params.row);
              setOpenDialog(true);
            }}
          >
            Edit
          </Button>
          <Button
            size="small"
            startIcon={<CalendarTodayIcon />}
            onClick={() => handleViewReservations(params.row)}
          >
            Reservations
          </Button>
          <Button
            size="small"
            color="error"
            startIcon={<DeleteIcon />}
            onClick={() => handleDelete(params.row)}
          >
            Delete
          </Button>
        </Box>
      ),
    }
  ];

  const fetchMembers = async () => {
    try {
      setLoading(true);
      const response = await memberAPI.getAll();
      setMembers(response.data);
      setError(null);
    } catch (error) {
      console.error('Error fetching members:', error);
      setError('Failed to load members');
      setMembers([]);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (member: Member) => {
    setMemberToDelete(member);
    setDeleteConfirmOpen(true);
  };

  const confirmDelete = async () => {
    if (!memberToDelete) return;
    
    try {
      await memberAPI.delete(memberToDelete.memberId);
      await fetchMembers();
      setError(null);
    } catch (error) {
      console.error('Error deleting member:', error);
      setError('Failed to delete member');
    } finally {
      setDeleteConfirmOpen(false);
      setMemberToDelete(null);
    }
  };

  const handleViewReservations = async (member: Member) => {
    try {
      const response = await memberAPI.getReservations(member.memberId);
      setReservations(response.data);
      setSelectedMember(member);
      setReservationDialogOpen(true);
    } catch (error) {
      console.error('Error loading reservations:', error);
      setError('Failed to load reservations');
    }
  };

  useEffect(() => {
    fetchMembers();
  }, []);

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h5">Members</Typography>
        <Button 
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => {
            setEditMember(null);
            setOpenDialog(true);
          }}
        >
          Add Member
        </Button>
      </Box>

      <Paper sx={{ width: '100%', overflow: 'hidden' }}>
        <DataGrid
          rows={members}
          columns={columns}
          getRowId={(row) => row.memberId}
          initialState={{
            pagination: {
              paginationModel: { pageSize: 10 }
            },
          }}
          pageSizeOptions={[10]}
          disableRowSelectionOnClick
          autoHeight
        />
      </Paper>

      <Dialog open={deleteConfirmOpen} onClose={() => setDeleteConfirmOpen(false)}>
        <DialogTitle>Confirm Delete</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete member{' '}
            {memberToDelete ? `${memberToDelete.firstName} ${memberToDelete.lastName}` : ''}?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirmOpen(false)}>Cancel</Button>
          <Button onClick={confirmDelete} variant="contained" color="error">
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      <MemberDialog
        open={openDialog}
        member={editMember}
        onClose={() => setOpenDialog(false)}
        onSave={async (data) => {
          try {
            if (editMember) {
              await memberAPI.update(editMember.memberId, {
                ...data,
                memberId: editMember.memberId
              });
            } else {
              await memberAPI.create(data);
            }
            await fetchMembers();
            setOpenDialog(false);
          } catch (error) {
            console.error('Error saving member:', error);
            setError('Failed to save member');
          }
        }}
      />

      <ReservationDialog
        open={reservationDialogOpen}
        onClose={() => setReservationDialogOpen(false)}
        reservations={reservations}
        memberName={selectedMember ? `${selectedMember.firstName} ${selectedMember.lastName}` : ''}
      />
    </Box>
  );
}
