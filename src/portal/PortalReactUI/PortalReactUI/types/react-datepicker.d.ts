declare module 'react-datepicker' {
    import { FC } from 'react';

    interface DatePickerProps {
        selected: Date | null;
        onChange: (date: Date | null) => void;
        minDate?: Date;
        maxDate?: Date;
        placeholderText?: string;
        id?: string;
        className?: string;
    }

    const DatePicker: FC<DatePickerProps>;
    export default DatePicker;
}
