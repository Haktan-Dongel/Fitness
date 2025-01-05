import { Dialog, DialogTitle, DialogContent, Table, TableBody, TableCell, 
         TableContainer, TableHead, TableRow, Paper } from '@mui/material';
import { MemberReservationView } from '../services/api';

interface ReservationDialogProps {
  open: boolean;
  onClose: () => void;
  reservations: MemberReservationView[];
  memberName?: string;
}

export default function ReservationDialog({ open, onClose, reservations, memberName }: ReservationDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        {memberName ? `Reservations for ${memberName}` : 'Reservations'}
      </DialogTitle>
      <DialogContent>
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>ID</TableCell>
                <TableCell>Date</TableCell>
                <TableCell>Equipment</TableCell>
                <TableCell>Time Slot</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {reservations.map((reservation) => (
                <TableRow key={reservation.reservationId}>
                  <TableCell>{reservation.reservationId}</TableCell>
                  <TableCell>{new Date(reservation.date).toLocaleDateString()}</TableCell>
                  <TableCell>{reservation.equipment}</TableCell>
                  <TableCell>{reservation.timeSlot}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </DialogContent>
    </Dialog>
  );
}
