import { useState, useEffect } from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, MenuItem, Alert,
  CircularProgress, FormControl, InputLabel, Select, Grid,
  List, ListItem, ListItemText, Checkbox, ListItemButton, Box, Typography  // Add Box and Typography imports
} from '@mui/material';
import { equipmentAPI, timeSlotAPI, Equipment as ApiEquipment } from '../services/api';

interface Equipment extends ApiEquipment {
  name: string;
}

interface TimeSlot {
  timeSlotId: number;
  startTime: string | Date;
  endTime: string | Date;
}

interface FormData {
  date: string;
  equipmentId: number;
  timeSlotIds: number[];
}

interface CreateReservationDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (data: FormData) => Promise<void>;
}

export default function CreateReservationDialog({ open, onClose, onSave }: CreateReservationDialogProps) {
  const [formData, setFormData] = useState<FormData>({
    date: new Date().toISOString().split('T')[0],
    equipmentId: 0,
    timeSlotIds: []
  });

  const [availableEquipment, setAvailableEquipment] = useState<Equipment[]>([]);
  const [availableTimeSlots, setAvailableTimeSlots] = useState<TimeSlot[]>([]);
  const [loading, setLoading] = useState(false);
  const [error] = useState<string | null>(null);

  useEffect(() => {
    if (open && formData.date && formData.equipmentId) {
      const fetchAvailability = async () => {
        setLoading(true);
        try {
          const availableTimeSlots = await timeSlotAPI.getAvailable(formData.date, formData.equipmentId);
          setAvailableTimeSlots(availableTimeSlots.data);
        } catch (error) {
          console.error('Error fetching time slots:', error);
        }
        setLoading(false);
      };

      fetchAvailability();
    }
  }, [open, formData.date, formData.equipmentId]);

  useEffect(() => {
    if (open) {
      const fetchEquipment = async () => {
        setLoading(true);
        try {
          const allEquipment = await equipmentAPI.getAll();
          setAvailableEquipment(allEquipment.data.map(e => ({
            ...e,
            name: e.deviceType
          })));
        } catch (error) {
          console.error('Error fetching equipment:', error);
        }
        setLoading(false);
      };

      fetchEquipment();
    }
  }, [open]);

  const handleReset = () => {
    // reset timeslot selectie
    setFormData(prev => ({
      ...prev,
      timeSlotIds: []
    }));
  };

  const formatTimeSlot = (timeSlot: TimeSlot): string => {
    const start = timeSlot.startTime.toString().split('T')[1]?.substring(0, 5) || timeSlot.startTime;
    const end = timeSlot.endTime.toString().split('T')[1]?.substring(0, 5) || timeSlot.endTime;
    return `${start} - ${end}`;
  };

  const findNextConsecutiveSlot = (currentSlot: TimeSlot): TimeSlot | undefined => {
    const currentEndTime = new Date(currentSlot.endTime).getTime();
    return availableTimeSlots.find(slot => 
      new Date(slot.startTime).getTime() === currentEndTime
    );
  };

  const handleTimeSlotToggle = (slot: TimeSlot) => {
    const currentSlots = [...formData.timeSlotIds];
    const slotId = slot.timeSlotId;
    
    if (currentSlots.includes(slotId)) {
      if (currentSlots[0] === slotId) {
        setFormData(prev => ({ ...prev, timeSlotIds: [] }));
      }
      else {
        setFormData(prev => ({
          ...prev,
          timeSlotIds: [currentSlots[0]]
        }));
      }
      return;
    }
    if (currentSlots.length === 0) {
      setFormData(prev => ({
        ...prev,
        timeSlotIds: [slotId]
      }));
      return;
    }

    const selectedSlot = availableTimeSlots.find(s => s.timeSlotId === currentSlots[0]);
    if (selectedSlot) {
      const nextConsecutive = findNextConsecutiveSlot(selectedSlot);
      if (nextConsecutive?.timeSlotId === slotId) {
        setFormData(prev => ({
          ...prev,
          timeSlotIds: [...prev.timeSlotIds, slotId]
        }));
      }
    }
  };

  const isSelectable = (slot: TimeSlot): boolean => {
    const currentSlots = formData.timeSlotIds;
    if (currentSlots.length === 0) return true;
    
    // check of de geselecteerde slot een volgende heeft
    if (currentSlots.length === 1) {
      const selectedSlot = availableTimeSlots.find(s => s.timeSlotId === currentSlots[0]);
      if (selectedSlot) {
        const nextConsecutive = findNextConsecutiveSlot(selectedSlot);
        return slot.timeSlotId === nextConsecutive?.timeSlotId;
      }
    }

    return false;
  };

  const isValidSelection = () => {
    return formData.date && 
           formData.equipmentId && 
           formData.timeSlotIds.length > 0 &&
           !error;
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Create New Reservation</DialogTitle>
      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        <Grid container spacing={2} sx={{ mt: 1 }}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              type="date"
              label="Date"
              value={formData.date}
              onChange={(e) => setFormData({ ...formData, date: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12}>
            <FormControl fullWidth>
              <InputLabel>Equipment</InputLabel>
              <Select
                value={formData.equipmentId || ''}
                onChange={(e) => {
                  setFormData({ 
                    ...formData, 
                    equipmentId: Number(e.target.value),
                    timeSlotIds: [] //reset timeslot selectie wanneer equipment verandert
                  });
                }}
                disabled={loading}
              >
                {availableEquipment.map((item) => (
                  <MenuItem key={item.equipmentId} value={item.equipmentId}>
                    {item.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          {formData.equipmentId > 0 && (
            <Grid item xs={12}>
              {loading ? (
                <Box display="flex" justifyContent="center">
                  <CircularProgress />
                </Box>
              ) : (
                <>
                  <Typography variant="subtitle1" gutterBottom>
                    Available Time Slots (Select up to 2 consecutive slots):
                  </Typography>
                  <List>
                    {availableTimeSlots.map((slot) => (
                      <ListItem
                        key={slot.timeSlotId}
                        disablePadding
                      >
                        <ListItemButton
                          dense
                          onClick={() => handleTimeSlotToggle(slot)}
                          disabled={!isSelectable(slot) && !formData.timeSlotIds.includes(slot.timeSlotId)}
                        >
                          <Checkbox
                            edge="start"
                            checked={formData.timeSlotIds.includes(slot.timeSlotId)}
                            disabled={!isSelectable(slot) && !formData.timeSlotIds.includes(slot.timeSlotId)}
                          />
                          <ListItemText 
                            primary={formatTimeSlot(slot)}
                            secondary={formData.timeSlotIds.length === 1 && !isSelectable(slot) ? 
                              "Not consecutive" : undefined}
                          />
                        </ListItemButton>
                      </ListItem>
                    ))}
                  </List>
                </>
              )}
            </Grid>
          )}
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button 
          onClick={handleReset}
          color="inherit"
          disabled={loading}
        >
          Reset
        </Button>
        <Button onClick={onClose}>Cancel</Button>
        <Button 
          onClick={() => onSave(formData)}
          disabled={loading || !isValidSelection()}
          color="primary"
          variant="contained"
        >
          {loading ? <CircularProgress size={24} /> : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
