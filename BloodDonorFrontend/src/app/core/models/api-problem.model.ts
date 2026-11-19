// The problem-details body the API returns for every error.
export interface ApiProblem {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  // Present on validation failures (400): field name -> messages.
  errors?: Record<string, string[]>;
}