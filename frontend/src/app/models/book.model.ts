/** Mirrors `MyLibrary.Domain.Enums.ReadingStatus`, serialized by the API as its string name. */
export type ReadingStatus = 'WantToRead' | 'Reading' | 'Read';

export const READING_STATUS_OPTIONS: { value: ReadingStatus; label: string }[] = [
  { value: 'WantToRead', label: 'Want to Read' },
  { value: 'Reading', label: 'Reading' },
  { value: 'Read', label: 'Read' }
];

export interface Book {
  id: string;
  title: string;
  author: string;
  genre: string | null;
  publicationYear: number | null;
  readingStatus: ReadingStatus | null;
  rating: number | null;
  notes: string | null;
}

export interface BookRequest {
  title: string;
  author: string;
  genre: string | null;
  publicationYear: number | null;
  readingStatus: ReadingStatus | null;
  rating: number | null;
  notes: string | null;
}
