declare module 'react-input-range' {
    import { FC } from 'react';

    interface InputRangeProps {
        maxValue: number;
        minValue: number;
        value: number | { min: number; max: number };
        onChange: (value: number | { min: number; max: number }) => void;
        onChangeComplete?: (value: number | { min: number; max: number }) => void;
        disabled?: boolean;
        formatLabel?: (value: number) => string;
        step?: number;
        draggableTrack?: boolean;
        allowSameValues?: boolean;
    }

    const InputRange: FC<InputRangeProps>;
    export default InputRange;
}
