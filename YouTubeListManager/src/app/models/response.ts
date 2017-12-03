interface IResponse {
  NextPageToken: string;
}

export class Response<T> implements IResponse {
  NextPageToken: string;
  Response: T[];
}
