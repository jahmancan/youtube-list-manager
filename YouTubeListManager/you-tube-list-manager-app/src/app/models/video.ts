import { PrivacyStatus } from './enums';

export class VideoInfo {
  Id: number;
  Hash: string;
  Title: string;
  Duration: number;
  Live: boolean;
  ThumbnailUrl: string;
  PrivacyStatus: PrivacyStatus;

}
